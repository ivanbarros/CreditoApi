using CreditoAPI.Application.Commands;
using FluentValidation;

namespace CreditoAPI.Application.Validators
{
    /// <summary>
    /// Validator para IntegrarCreditosCommand
    /// Implementa validação robusta seguindo princípios SOLID
    /// </summary>
    public class IntegrarCreditosCommandValidator : AbstractValidator<IntegrarCreditosCommand>
    {
        public IntegrarCreditosCommandValidator()
        {
            RuleFor(x => x.Creditos)
                .NotNull()
                .WithMessage("A lista de créditos não pode ser nula")
                .NotEmpty()
                .WithMessage("A lista de créditos não pode estar vazia");

            RuleForEach(x => x.Creditos)
                .SetValidator(new CreditoDtoValidator());
        }
    }

    /// <summary>
    /// Validator para CreditoDto
    /// </summary>
    public class CreditoDtoValidator : AbstractValidator<DTOs.CreditoDto>
    {
        public CreditoDtoValidator()
        {
            RuleFor(x => x.NumeroCredito)
                .NotEmpty()
                .WithMessage("Número do crédito é obrigatório")
                .MaximumLength(50)
                .WithMessage("Número do crédito deve ter no máximo 50 caracteres");

            RuleFor(x => x.NumeroNfse)
                .NotEmpty()
                .WithMessage("Número da NFS-e é obrigatório")
                .MaximumLength(50)
                .WithMessage("Número da NFS-e deve ter no máximo 50 caracteres");

            RuleFor(x => x.DataConstituicao)
                .NotEmpty()
                .WithMessage("Data de constituição é obrigatória")
                .Must(BeAValidDate)
                .WithMessage("Data de constituição deve estar no formato yyyy-MM-dd");

            RuleFor(x => x.ValorIssqn)
                .GreaterThan(0)
                .WithMessage("Valor do ISSQN deve ser maior que zero");

            RuleFor(x => x.TipoCredito)
                .NotEmpty()
                .WithMessage("Tipo de crédito é obrigatório")
                .MaximumLength(50)
                .WithMessage("Tipo de crédito deve ter no máximo 50 caracteres");

            RuleFor(x => x.SimplesNacional)
                .NotEmpty()
                .WithMessage("Simples Nacional é obrigatório")
                .Must(x => x == "Sim" || x == "Não")
                .WithMessage("Simples Nacional deve ser 'Sim' ou 'Não'");

            RuleFor(x => x.Aliquota)
                .GreaterThan(0)
                .WithMessage("Alíquota deve ser maior que zero")
                .LessThanOrEqualTo(100)
                .WithMessage("Alíquota deve ser menor ou igual a 100");

            RuleFor(x => x.ValorFaturado)
                .GreaterThan(0)
                .WithMessage("Valor faturado deve ser maior que zero");

            RuleFor(x => x.ValorDeducao)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Valor de dedução não pode ser negativo");

            RuleFor(x => x.BaseCalculo)
                .GreaterThan(0)
                .WithMessage("Base de cálculo deve ser maior que zero");
        }

        private bool BeAValidDate(string date)
        {
            return DateTime.TryParseExact(date, "yyyy-MM-dd", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out _);
        }
    }
}
