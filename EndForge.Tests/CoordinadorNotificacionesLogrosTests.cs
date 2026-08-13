using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CoordinadorNotificacionesLogrosTests {
    private static readonly DateTimeOffset FechaBase = new(
        2026,
        8,
        9,
        12,
        0,
        0,
        TimeSpan.Zero);

    [Fact]
    public void Registrar_UnLogroActual_QuedaPendiente() {
        CoordinadorNotificacionesLogros coordinador = new();

        bool agregado = coordinador.Registrar(CrearLogro("logro:uno"));
        IReadOnlyList<LogroDesbloqueado> pendientes =
            coordinador.ConsultarPendientes();

        Assert.True(agregado);
        Assert.Equal(1, coordinador.CantidadPendientes);
        Assert.Equal("logro:uno", Assert.Single(pendientes).LogroId);
    }

    [Fact]
    public void Registrar_VariosLogros_LosAgrupaEnOrdenDeLlegada() {
        CoordinadorNotificacionesLogros coordinador = new();

        int agregados = coordinador.Registrar(new[] {
            CrearLogro("logro:uno"),
            CrearLogro("logro:dos", FechaBase.AddMinutes(1)),
            CrearLogro("logro:tres", FechaBase.AddMinutes(2))
        });
        IReadOnlyList<LogroDesbloqueado> consumidos =
            coordinador.ConsumirPendientes();

        Assert.Equal(3, agregados);
        Assert.Equal(
            new[] { "logro:uno", "logro:dos", "logro:tres" },
            consumidos.Select(logro => logro.LogroId));
        Assert.Equal(0, coordinador.CantidadPendientes);
    }

    [Fact]
    public void Registrar_DuplicadosConDistintaCapitalizacion_ConservaSoloElPrimero() {
        CoordinadorNotificacionesLogros coordinador = new();

        Assert.True(coordinador.Registrar(CrearLogro("logro:Curso:Uno")));
        Assert.False(coordinador.Registrar(CrearLogro(" LOGRO:curso:uno ")));

        LogroDesbloqueado pendiente = Assert.Single(
            coordinador.ConsumirPendientes());
        Assert.Equal("logro:Curso:Uno", pendiente.LogroId);
    }

    [Fact]
    public void Registrar_LogroImportado_NoLoPresentaNiPermiteReinyectarlo() {
        CoordinadorNotificacionesLogros coordinador = new();

        Assert.False(coordinador.Registrar(
            CrearLogro("logro:importado", esImportado: true)));
        Assert.False(coordinador.Registrar(CrearLogro("logro:importado")));

        Assert.Empty(coordinador.ConsultarPendientes());
    }

    [Fact]
    public void RecargaMismoLogro_DespuesDeConsumir_NoLoReinyecta() {
        CoordinadorNotificacionesLogros coordinador = new();
        LogroDesbloqueado logro = CrearLogro("logro:estable");

        Assert.True(coordinador.Registrar(logro));
        Assert.Single(coordinador.ConsumirPendientes());

        Assert.False(coordinador.Registrar(CrearLogro("LOGRO:ESTABLE")));
        Assert.Empty(coordinador.ConsumirPendientes());
    }

    [Fact]
    public void InicioOculto_ConsultarSinConsumir_ConservaLosPendientes() {
        CoordinadorNotificacionesLogros coordinador = new();
        coordinador.Registrar(new[] {
            CrearLogro("logro:uno"),
            CrearLogro("logro:dos")
        });

        IReadOnlyList<LogroDesbloqueado> primeraConsulta =
            coordinador.ConsultarPendientes();
        IReadOnlyList<LogroDesbloqueado> segundaConsulta =
            coordinador.ConsultarPendientes();

        Assert.Equal(2, primeraConsulta.Count);
        Assert.Equal(2, segundaConsulta.Count);
        Assert.Equal(2, coordinador.CantidadPendientes);
        Assert.Equal(2, coordinador.ConsumirPendientes().Count);
    }

    [Fact]
    public void PresentacionInterrumpida_ReponeElLoteSinDuplicarlo() {
        CoordinadorNotificacionesLogros coordinador = new();
        coordinador.Registrar(new[] {
            CrearLogro("logro:uno"),
            CrearLogro("logro:dos")
        });
        IReadOnlyList<LogroDesbloqueado> loteVisible =
            coordinador.ConsumirPendientes();
        coordinador.Registrar(CrearLogro("logro:tres"));

        int repuestos = coordinador.ReponerPendientesAlInicio(loteVisible);
        int repetidos = coordinador.ReponerPendientesAlInicio(loteVisible);

        Assert.Equal(2, repuestos);
        Assert.Equal(0, repetidos);
        Assert.Equal(
            new[] { "logro:uno", "logro:dos", "logro:tres" },
            coordinador.ConsumirPendientes().Select(logro => logro.LogroId));
    }

    [Fact]
    public void Reponer_TrasCerrarNoRestauraPendientes() {
        CoordinadorNotificacionesLogros coordinador = new();
        coordinador.Registrar(CrearLogro("logro:uno"));
        IReadOnlyList<LogroDesbloqueado> lote =
            coordinador.ConsumirPendientes();

        coordinador.Cerrar();

        Assert.Equal(0, coordinador.ReponerPendientesAlInicio(lote));
        Assert.Empty(coordinador.ConsultarPendientes());
    }

    [Fact]
    public void Cerrar_LimpiaPendientesEsIdempotenteYRechazaNuevos() {
        CoordinadorNotificacionesLogros coordinador = new();
        coordinador.Registrar(CrearLogro("logro:uno"));

        coordinador.Cerrar();
        coordinador.Cerrar();

        Assert.True(coordinador.Cerrado);
        Assert.Equal(0, coordinador.CantidadPendientes);
        Assert.Empty(coordinador.ConsultarPendientes());
        Assert.Empty(coordinador.ConsumirPendientes());
        Assert.False(coordinador.Registrar(CrearLogro("logro:dos")));
        Assert.Equal(0, coordinador.Registrar(new[] {
            CrearLogro("logro:tres")
        }));
    }

    [Fact]
    public void Consultar_DevuelveCopiasQueNoAlteranElEstadoInterno() {
        CoordinadorNotificacionesLogros coordinador = new();
        coordinador.Registrar(CrearLogro("logro:original"));

        LogroDesbloqueado copia = Assert.Single(
            coordinador.ConsultarPendientes());
        copia.LogroId = "logro:modificado";
        copia.EsImportado = true;

        LogroDesbloqueado interno = Assert.Single(
            coordinador.ConsumirPendientes());
        Assert.Equal("logro:original", interno.LogroId);
        Assert.False(interno.EsImportado);
    }

    [Fact]
    public async Task RegistrarConcurrentemente_MismoId_SoloAgregaUnaVez() {
        CoordinadorNotificacionesLogros coordinador = new();

        bool[] resultados = await Task.WhenAll(
            Enumerable.Range(0, 40)
                .Select(indice => Task.Run(() => coordinador.Registrar(
                    CrearLogro(indice % 2 == 0
                        ? "logro:concurrente"
                        : "LOGRO:CONCURRENTE")))));

        Assert.Equal(1, resultados.Count(resultado => resultado));
        Assert.Single(coordinador.ConsumirPendientes());
    }

    private static LogroDesbloqueado CrearLogro(
        string id,
        DateTimeOffset? fecha = null,
        bool esImportado = false) {
        return new LogroDesbloqueado {
            LogroId = id,
            FechaReconocimientoUtc = fecha ?? FechaBase,
            EsImportado = esImportado
        };
    }
}
