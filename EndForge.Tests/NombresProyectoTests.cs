using EndForge.Services;

namespace EndForge.Tests;

public sealed class NombresProyectoTests {
    [Theory]
    [InlineData("Mi proyecto")]
    [InlineData("Árbol")]
    [InlineData("Niñez")]
    [InlineData("con-guion")]
    [InlineData("versión.2")]
    public void AceptaNombresSegurosConUnicodeYSeparadoresComunes(string nombre) {
        Assert.True(new NombrePracticaService().Validar(nombre).EsValido);
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("aux.txt")]
    [InlineData("termina.")]
    [InlineData("termina ")]
    [InlineData("A&B")]
    [InlineData("A<B")]
    public void RechazaNombresIncompatiblesConWindowsOXml(string nombre) {
        Assert.False(new NombrePracticaService().Validar(nombre).EsValido);
    }
}
