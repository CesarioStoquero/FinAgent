using System.Diagnostics;

namespace FinAgent.Wallet.IntegrationTests.Fixtures;

/// <summary>
/// Convenção do projeto para testes `[requer contêiner]` (Testcontainers): antes de
/// qualquer coisa que dependa de Docker, o teste chama
/// <c>Assert.SkipUnless(DockerAvailability.IsAvailable, "...")</c>. Isso faz o teste
/// aparecer como IGNORADO (não como falha) quando não há Docker na máquina — é o que
/// mantém `dotnet test` verde sem container, sem esconder o teste do relatório.
///
/// Detecção: tenta rodar <c>docker info</c>. Sem o executável ou sem daemon, falha e
/// o resultado (calculado uma única vez por execução) é <see langword="false"/>.
/// </summary>
internal static class DockerAvailability
{
    private static readonly Lazy<bool> Detected = new(Detect);

    public static bool IsAvailable => Detected.Value;

    private static bool Detect()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process is null)
            {
                return false;
            }

            var exited = process.WaitForExit(5_000);
            return exited && process.ExitCode == 0;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            // "docker" não existe no PATH desta máquina (ambiente sem Docker instalado).
            return false;
        }
    }
}
