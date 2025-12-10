using CreditoAPI.Application.Commands;
using CreditoAPI.Application.Queries;
using CreditoAPI.DTOs;
using CreditoAPI.Infrastructure.Cache;
using CreditoAPI.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace CreditoAPI.Controllers
{
    /// <summary>
    /// Controller de Créditos usando CQRS Pattern com MediatR
    /// Implementa SOLID - Single Responsibility e Dependency Inversion
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/creditos")]
    [ApiVersion("1.0")]
    [Authorize]
    public class CreditosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreditosController> _logger;
        private readonly IServiceBusService _serviceBusService;
        private readonly ICacheService? _cacheService;

        public CreditosController(
            IMediator mediator, 
            ILogger<CreditosController> logger, 
            IServiceBusService serviceBusService,
            ICacheService? cacheService = null)
        {
            _mediator = mediator;
            _logger = logger;
            _serviceBusService = serviceBusService;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Integra uma lista de créditos constituídos na base de dados via Service Bus
        /// </summary>
        /// <param name="creditos">Lista de créditos a serem integrados</param>
        /// <returns>Status da operação</returns>
        [HttpPost("integrar-credito-constituido")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IntegrarCreditoResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IntegrarCreditoConstituido([FromBody] List<CreditoDto> creditos)
        {
            try
            {
                if (creditos == null || !creditos.Any())
                {
                    return BadRequest(new { error = "Lista de créditos não pode ser vazia" });
                }

                _logger.LogInformation("Received request to integrate {Count} creditos", creditos.Count);

                // CQRS - Envia comando via MediatR
                var command = new IntegrarCreditosCommand(creditos);
                var result = await _mediator.Send(command);

                return Accepted(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error integrating creditos");
                return StatusCode(500, new { error = "Erro ao integrar créditos" });
            }
        }

        /// <summary>
        /// Retorna uma lista de créditos constituídos com base no número da NFS-e
        /// </summary>
        /// <param name="numeroNfse">Número identificador da NFS-e</param>
        /// <returns>Lista de créditos</returns>
        [HttpGet("{numeroNfse}")]
        [ProducesResponseType(typeof(PagedResult<CreditoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByNumeroNfse(
            string numeroNfse,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving creditos by numero nfse: {NumeroNfse}", numeroNfse);

                var cacheKey = $"creditos:nfse:{numeroNfse}:page:{pageNumber}:size:{pageSize}";
                
                if (_cacheService != null)
                {
                    var cachedResult = await _cacheService.GetAsync<PagedResult<CreditoDto>>(cacheKey);
                    if (cachedResult != null)
                    {
                        _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                        return Ok(cachedResult);
                    }
                }

                var query = new GetCreditosByNfseQuery(numeroNfse);
                var allCreditos = await _mediator.Send(query);

                if (allCreditos == null || !allCreditos.Any())
                {
                    return NotFound(new { error = "Nenhum crédito encontrado para a NFS-e informada" });
                }

                var totalCount = allCreditos.Count;
                var items = allCreditos
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResult<CreditoDto>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                if (_cacheService != null)
                {
                    await _cacheService.SetAsync(cacheKey, pagedResult, TimeSpan.FromMinutes(5));
                }

                await _serviceBusService.SendAuditMessageAsync("consulta-por-nfse", numeroNfse);

                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving creditos by numero nfse: {NumeroNfse}", numeroNfse);
                return StatusCode(500, new { error = "Erro ao buscar créditos" });
            }
        }

        /// <summary>
        /// Retorna os detalhes de um crédito constituído específico com base no número do crédito
        /// </summary>
        /// <param name="numeroCredito">Número identificador do crédito constituído</param>
        /// <returns>Detalhes do crédito</returns>
        [HttpGet("credito/{numeroCredito}")]
        [ProducesResponseType(typeof(CreditoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByNumeroCredito(string numeroCredito)
        {
            try
            {
                _logger.LogInformation("Retrieving credito by numero credito: {NumeroCredito}", numeroCredito);

                var cacheKey = $"credito:{numeroCredito}";
                
                if (_cacheService != null)
                {
                    var cachedCredito = await _cacheService.GetAsync<CreditoDto>(cacheKey);
                    if (cachedCredito != null)
                    {
                        _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                        return Ok(cachedCredito);
                    }
                }

                var query = new GetCreditoByNumeroQuery(numeroCredito);
                var credito = await _mediator.Send(query);

                if (credito == null)
                {
                    return NotFound(new { error = "Crédito não encontrado" });
                }

                if (_cacheService != null)
                {
                    await _cacheService.SetAsync(cacheKey, credito, TimeSpan.FromMinutes(10));
                }

                await _serviceBusService.SendAuditMessageAsync("consulta-por-credito", numeroCredito);

                return Ok(credito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving credito by numero credito: {NumeroCredito}", numeroCredito);
                return StatusCode(500, new { error = "Erro ao buscar crédito" });
            }
        }
    }
}
