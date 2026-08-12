namespace EndForge.Tests;

public sealed class CargaNavegacionTests {
    [Fact]
    public void ResultadoVigente_SePuedeAplicar() {
        Assert.True(PuedeAplicar());
    }

    [Theory]
    [InlineData(false, true, true, false, false, false, true, true)]
    [InlineData(true, false, true, false, false, false, true, true)]
    [InlineData(true, true, false, false, false, false, true, true)]
    [InlineData(true, true, true, true, false, false, true, true)]
    [InlineData(true, true, true, false, true, false, true, true)]
    [InlineData(true, true, true, false, false, true, true, true)]
    [InlineData(true, true, true, false, false, false, false, true)]
    [InlineData(true, true, true, false, false, false, true, false)]
    public void ResultadoObsoletoOCierre_NoActualizaControles(
        bool resultadoDisponible,
        bool puedeActualizarInterfaz,
        bool handleCreado,
        bool formularioEliminado,
        bool formularioEliminandose,
        bool esperandoCierre,
        bool seleccionVigente,
        bool secuenciaVigente) {
        Assert.False(frmPrincipal.PuedeAplicarCargaNavegacion(
            resultadoDisponible,
            puedeActualizarInterfaz,
            handleCreado,
            formularioEliminado,
            formularioEliminandose,
            esperandoCierre,
            seleccionVigente,
            secuenciaVigente));
    }

    [Fact]
    public async Task SolicitudesCompletadasEnOrdenInverso_SoloAplicaLaUltima() {
        const long primeraSolicitud = 1;
        const long ultimaSolicitud = 2;
        TaskCompletionSource primeraLiberada = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource ultimaLiberada = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        List<long> solicitudesAplicadas = [];

        Task primera = AplicarCuandoTermineAsync(
            primeraSolicitud,
            ultimaSolicitud,
            primeraLiberada.Task,
            solicitudesAplicadas);
        Task ultima = AplicarCuandoTermineAsync(
            ultimaSolicitud,
            ultimaSolicitud,
            ultimaLiberada.Task,
            solicitudesAplicadas);

        ultimaLiberada.SetResult();
        await ultima;
        primeraLiberada.SetResult();
        await primera;

        Assert.Equal([ultimaSolicitud], solicitudesAplicadas);
    }

    [Theory]
    [InlineData(true, true, "grado-1", "GRADO-1", true)]
    [InlineData(false, true, "grado-1", "grado-1", false)]
    [InlineData(true, false, "grado-1", "grado-1", false)]
    [InlineData(true, true, "grado-1", "grado-2", false)]
    [InlineData(true, true, null, "grado-1", false)]
    public void CargaEstadisticas_SoloSeComparteParaElMismoGradoActivo(
        bool cargaActiva,
        bool mismoCatalogo,
        string? gradoCarga,
        string gradoSolicitado,
        bool esperado) {
        Assert.Equal(
            esperado,
            frmPrincipal.PuedeReutilizarCargaEstadisticas(
                cargaActiva,
                mismoCatalogo,
                gradoCarga,
                gradoSolicitado));
    }

    private static bool PuedeAplicar() {
        return frmPrincipal.PuedeAplicarCargaNavegacion(
            resultadoDisponible: true,
            puedeActualizarInterfaz: true,
            handleCreado: true,
            formularioEliminado: false,
            formularioEliminandose: false,
            esperandoCierre: false,
            seleccionVigente: true,
            secuenciaVigente: true);
    }

    private static async Task AplicarCuandoTermineAsync(
        long solicitud,
        long ultimaSolicitud,
        Task espera,
        ICollection<long> solicitudesAplicadas) {
        await espera;

        if (frmPrincipal.EsSolicitudCargaNavegacionVigente(
                solicitud,
                ultimaSolicitud)) {
            solicitudesAplicadas.Add(solicitud);
        }
    }
}
