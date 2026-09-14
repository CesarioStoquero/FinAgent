using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinAgent.Wallet.Infrastructure.Persistence.Migrations;

/// <summary>
/// Cria o event store (ADR-0001, ADR-0007) e a permissão que sustenta RN-3 (o passado é
/// fisicamente imutável): a role <c>wallet_app</c> recebe só SELECT/INSERT em
/// <c>wallet_events</c>; UPDATE/DELETE são revogados de propósito (ADR-0011 seção 5).
/// GRANT/REVOKE/CREATE ROLE são objetos de cluster que o EF não modela — entram via
/// <see cref="MigrationBuilder.Sql(string, bool, string)"/>. Só a criação da role
/// continua condicional (pode preexistir); tabela e índices nunca levam
/// <c>IF NOT EXISTS</c> (ADR-0011 seção 5).
/// </summary>
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "wallet_events",
            columns: table => new
            {
                aggregate_id = table.Column<Guid>(type: "uuid", nullable: false),
                version = table.Column<int>(type: "integer", nullable: false),
                global_seq = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                event_type = table.Column<string>(type: "text", nullable: false),
                payload = table.Column<string>(type: "jsonb", nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_wallet_events", x => new { x.aggregate_id, x.version });
            });

        migrationBuilder.CreateIndex(
            name: "ix_wallet_events_aggregate_id",
            table: "wallet_events",
            column: "aggregate_id");

        migrationBuilder.CreateIndex(
            name: "ux_wallet_events_global_seq",
            table: "wallet_events",
            column: "global_seq",
            unique: true);

        // RN-3: o passado é fisicamente imutável. A role da aplicação recebe só
        // SELECT/INSERT; UPDATE/DELETE são revogados de propósito (nenhum código do
        // projeto jamais deveria precisar deles). Role é objeto de CLUSTER e pode
        // preexistir num banco recém-criado — só ela fica condicional.
        migrationBuilder.Sql(
            """
            DO
            $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'wallet_app') THEN
                        CREATE ROLE wallet_app LOGIN PASSWORD 'wallet_app';
                    END IF;
                END
            $$;
            """);

        migrationBuilder.Sql("GRANT USAGE ON SCHEMA public TO wallet_app;");
        migrationBuilder.Sql("GRANT SELECT, INSERT ON wallet_events TO wallet_app;");
        migrationBuilder.Sql("GRANT USAGE, SELECT ON SEQUENCE wallet_events_global_seq_seq TO wallet_app;");
        migrationBuilder.Sql("REVOKE UPDATE, DELETE ON wallet_events FROM wallet_app;");
        migrationBuilder.Sql("REVOKE UPDATE, DELETE ON wallet_events FROM PUBLIC;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // A role `wallet_app` não é derrubada aqui: ela é objeto de cluster, pode ser
        // compartilhada por outros bancos, e sua criação já é condicional na Up (pode
        // ter preexistido). Derrubar a tabela já torna moot os GRANT/REVOKE sobre ela.
        migrationBuilder.DropTable(
            name: "wallet_events");
    }
}
