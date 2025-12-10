using CreditoAPI.Application.Commands;
using CreditoAPI.Application.Queries;
using CreditoAPI.DTOs;
using CreditoAPI.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CreditoAPI.Controllers
{
    /// <summary>
    /// Controller de Créditos usando CQRS Pattern com MediatR
    /// Implementa SOLID - Single Responsibility e Dependency Inversion
    /// </summary>
    [ApiController]
    [Route("api/creditos")]
    public class CreditosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreditosController> _logger;
        private readonly IServiceBusService _serviceBusService;
        private ICreditoService object1;
        private ILogger<CreditosController> object2;

        public CreditosController(IMediator mediator, ILogger<CreditosController> logger, IServiceBusService serviceBusService)
        {
            _mediator = mediator;
            _logger = logger;
            _serviceBusService = serviceBusService;
        }

        public CreditosController(ICreditoService object1, ILogger<CreditosController> object2)
        {
            this.object1 = object1;
            this.object2 = object2;
        }

        /// <summary>
        /// Integra uma lista de créditos constituídos na base de dados via Service Bus
        /// </summary>
        /// <param name="creditos">Lista de créditos a serem integrados</param>
        /// <returns>Status da operação</returns>
        [HttpPost("integrar-credito-constituido")]
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
        [ProducesResponseType(typeof(List<CreditoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByNumeroNfse(string numeroNfse)
        {
            try
            {
                _logger.LogInformation("Retrieving creditos by numero nfse: {NumeroNfse}", numeroNfse);

                // CQRS - Envia query via MediatR
                var query = new GetCreditosByNfseQuery(numeroNfse);
                var creditos = await _mediator.Send(query);

                if (creditos == null || !creditos.Any())
                {
                    return NotFound(new { error = "Nenhum crédito encontrado para a NFS-e informada" });
                }

                await _serviceBusService.SendAuditMessageAsync("consulta-por-nfse", numeroNfse);

                return Ok(creditos);
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

                // CQRS - Envia query via MediatR
                var query = new GetCreditoByNumeroQuery(numeroCredito);
                var credito = await _mediator.Send(query);

                if (credito == null)
                {
                    return NotFound(new { error = "Crédito não encontrado" });
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
