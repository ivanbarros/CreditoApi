using System;
using CreditoAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CreditoAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.25")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CreditoAPI.Models.Credito", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<decimal>("Aliquota")
                        .HasPrecision(5, 2)
                        .HasColumnType("numeric(5,2)")
                        .HasColumnName("aliquota");

                    b.Property<decimal>("BaseCalculo")
                        .HasPrecision(15, 2)
                        .HasColumnType("numeric(15,2)")
                        .HasColumnName("base_calculo");

                    b.Property<DateTime>("DataConstituicao")
                        .HasColumnType("date")
                        .HasColumnName("data_constituicao");

                    b.Property<string>("NumeroCredito")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("numero_credito");

                    b.Property<string>("NumeroNfse")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("numero_nfse");

                    b.Property<bool>("SimplesNacional")
                        .HasColumnType("boolean")
                        .HasColumnName("simples_nacional");

                    b.Property<string>("TipoCredito")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("tipo_credito");

                    b.Property<decimal>("ValorDeducao")
                        .HasPrecision(15, 2)
                        .HasColumnType("numeric(15,2)")
                        .HasColumnName("valor_deducao");

                    b.Property<decimal>("ValorFaturado")
                        .HasPrecision(15, 2)
                        .HasColumnType("numeric(15,2)")
                        .HasColumnName("valor_faturado");

                    b.Property<decimal>("ValorIssqn")
                        .HasPrecision(15, 2)
                        .HasColumnType("numeric(15,2)")
                        .HasColumnName("valor_issqn");

                    b.HasKey("Id");

                    b.HasIndex("NumeroCredito")
                        .HasDatabaseName("idx_numero_credito");

                    b.HasIndex("NumeroNfse")
                        .HasDatabaseName("idx_numero_nfse");

                    b.ToTable("credito", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
