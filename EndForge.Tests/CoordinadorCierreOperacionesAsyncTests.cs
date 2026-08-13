namespace EndForge.Tests;

public sealed class CoordinadorCierreOperacionesAsyncTests {
    [Fact]
    public void CierreConOperacionActiva_SeCancelaYEspera() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> operacion = CrearFuente();

        DecisionCierreOperacionesAsync decision =
            coordinador.SolicitarCierre(operacion.Task);

        Assert.False(decision.PermitirCierre);
        Assert.True(decision.DebeEsperar);
        Assert.NotNull(decision.FinalizacionPendiente);
        Assert.False(decision.FinalizacionPendiente.IsCompleted);
        Assert.True(coordinador.CierreSolicitado);
    }

    [Fact]
    public async Task PersistenciaBloqueada_MantienePendienteElCierre() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> evaluacion = CrearFuente();
        TaskCompletionSource<bool> persistencia = CrearFuente();
        TaskCompletionSource<bool> persistenciaIniciada =
            CrearFuente();
        Task flujo = CrearFlujoCompletoAsync(
            evaluacion.Task,
            persistencia.Task,
            persistenciaIniciada);
        DecisionCierreOperacionesAsync decision =
            coordinador.SolicitarCierre(flujo);

        evaluacion.SetResult(true);
        await persistenciaIniciada.Task;

        Assert.False(flujo.IsCompleted);
        Assert.False(decision.FinalizacionPendiente!.IsCompleted);
        Assert.False(coordinador.IntentarAutorizarReintento());
    }

    [Fact]
    public async Task MotivacionBloqueada_FormaParteDelFlujoCompletoDeEvaluacion() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> evaluacion = CrearFuente();
        TaskCompletionSource<bool> persistencia = CrearFuente();
        TaskCompletionSource<bool> motivacion = CrearFuente();
        TaskCompletionSource<bool> motivacionIniciada = CrearFuente();
        Task flujo = CrearFlujoConMotivacionAsync(
            evaluacion.Task,
            persistencia.Task,
            motivacion.Task,
            motivacionIniciada);
        DecisionCierreOperacionesAsync decision =
            coordinador.SolicitarCierre(flujo);

        evaluacion.SetResult(true);
        persistencia.SetResult(true);
        await motivacionIniciada.Task;

        Assert.False(flujo.IsCompleted);
        Assert.False(decision.FinalizacionPendiente!.IsCompleted);
        Assert.False(coordinador.IntentarAutorizarReintento());

        motivacion.SetResult(true);
        await decision.FinalizacionPendiente;

        Assert.True(coordinador.IntentarAutorizarReintento());
        Assert.False(coordinador.IntentarAutorizarReintento());
    }

    [Fact]
    public async Task MotivacionIndependienteComoTerceraOperacion_ImpideCerrar() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> motivacion = CrearFuente();

        DecisionCierreOperacionesAsync primera = coordinador.SolicitarCierre(
            Task.CompletedTask,
            Task.CompletedTask,
            motivacion.Task);
        DecisionCierreOperacionesAsync segunda = coordinador.SolicitarCierre(
            Task.CompletedTask,
            Task.CompletedTask,
            motivacion.Task);

        Assert.True(primera.DebeEsperar);
        Assert.False(segunda.PermitirCierre);
        Assert.False(segunda.DebeEsperar);
        Assert.False(coordinador.PuedeActualizarInterfaz);

        motivacion.SetResult(true);
        await primera.FinalizacionPendiente!;

        Assert.True(coordinador.IntentarAutorizarReintento());
    }

    [Fact]
    public async Task CierreContinuaCanceladoMientrasPersiste() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> evaluacion = CrearFuente();
        TaskCompletionSource<bool> persistencia = CrearFuente();
        TaskCompletionSource<bool> persistenciaIniciada =
            CrearFuente();
        Task flujo = CrearFlujoCompletoAsync(
            evaluacion.Task,
            persistencia.Task,
            persistenciaIniciada);
        DecisionCierreOperacionesAsync primera =
            coordinador.SolicitarCierre(flujo);

        evaluacion.SetResult(true);
        await persistenciaIniciada.Task;
        DecisionCierreOperacionesAsync segunda =
            coordinador.SolicitarCierre(flujo);

        Assert.False(primera.PermitirCierre);
        Assert.False(segunda.PermitirCierre);
        Assert.False(segunda.DebeEsperar);
        Assert.Null(segunda.FinalizacionPendiente);

        persistencia.SetResult(true);
        await primera.FinalizacionPendiente!;
    }

    [Fact]
    public async Task PersistenciaFinalizada_PermiteElReintento() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> operacion = CrearFuente();
        DecisionCierreOperacionesAsync primera =
            coordinador.SolicitarCierre(operacion.Task);

        operacion.SetResult(true);
        await primera.FinalizacionPendiente!;

        Assert.True(coordinador.IntentarAutorizarReintento());

        DecisionCierreOperacionesAsync reintento =
            coordinador.SolicitarCierre(operacion.Task);
        Assert.True(reintento.PermitirCierre);
        Assert.False(reintento.DebeEsperar);
    }

    [Fact]
    public async Task ReintentoDeCierre_SeProgramaUnaSolaVez() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> operacion = CrearFuente();
        DecisionCierreOperacionesAsync decision =
            coordinador.SolicitarCierre(operacion.Task);

        coordinador.SolicitarCierre(operacion.Task);
        coordinador.SolicitarCierre(operacion.Task);
        operacion.SetResult(true);
        await decision.FinalizacionPendiente!;

        Assert.True(coordinador.IntentarAutorizarReintento());
        Assert.False(coordinador.IntentarAutorizarReintento());
    }

    [Fact]
    public void CierreSolicitado_ImpideActualizarInterfaz() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> operacion = CrearFuente();

        Assert.True(coordinador.PuedeActualizarInterfaz);

        coordinador.SolicitarCierre(operacion.Task);

        Assert.False(coordinador.PuedeActualizarInterfaz);
    }

    [Fact]
    public async Task ExcepcionDurantePersistencia_NoImpideAutorizarCierre() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> evaluacion = CrearFuente();
        TaskCompletionSource<bool> persistencia = CrearFuente();
        TaskCompletionSource<bool> persistenciaIniciada =
            CrearFuente();
        Task flujo = CrearFlujoCompletoAsync(
            evaluacion.Task,
            persistencia.Task,
            persistenciaIniciada);
        DecisionCierreOperacionesAsync decision =
            coordinador.SolicitarCierre(flujo);

        evaluacion.SetResult(true);
        await persistenciaIniciada.Task;
        persistencia.SetException(
            new IOException("Fallo de persistencia"));

        await Assert.ThrowsAsync<IOException>(
            () => decision.FinalizacionPendiente!);
        Assert.True(coordinador.IntentarAutorizarReintento());
    }

    [Fact]
    public void SolicitudesMultiples_NoCreanEsperasAdicionales() {
        CoordinadorCierreOperacionesAsync coordinador = new();
        TaskCompletionSource<bool> operacion = CrearFuente();

        DecisionCierreOperacionesAsync primera =
            coordinador.SolicitarCierre(operacion.Task);
        DecisionCierreOperacionesAsync segunda =
            coordinador.SolicitarCierre(operacion.Task);
        DecisionCierreOperacionesAsync tercera =
            coordinador.SolicitarCierre(operacion.Task);

        Assert.True(primera.DebeEsperar);
        Assert.False(segunda.DebeEsperar);
        Assert.False(tercera.DebeEsperar);
        Assert.Null(segunda.FinalizacionPendiente);
        Assert.Null(tercera.FinalizacionPendiente);
    }

    [Fact]
    public void OperacionNormalSinCierre_ConservaActualizacionDeInterfaz() {
        CoordinadorCierreOperacionesAsync coordinador = new();

        Assert.True(coordinador.PuedeActualizarInterfaz);
        Assert.False(coordinador.CierreSolicitado);
    }

    private static TaskCompletionSource<bool> CrearFuente() {
        return new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private static async Task CrearFlujoCompletoAsync(
        Task evaluacion,
        Task persistencia,
        TaskCompletionSource<bool> persistenciaIniciada) {
        await evaluacion;
        persistenciaIniciada.SetResult(true);
        await persistencia;
    }

    private static async Task CrearFlujoConMotivacionAsync(
        Task evaluacion,
        Task persistencia,
        Task motivacion,
        TaskCompletionSource<bool> motivacionIniciada) {
        await evaluacion;
        await persistencia;
        motivacionIniciada.SetResult(true);
        await motivacion;
    }
}
