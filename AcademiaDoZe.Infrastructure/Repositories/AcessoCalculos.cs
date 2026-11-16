// Luan Coelho

namespace AcademiaDoZe.Infrastructure.Repositories;

/*
Os acessos são gravados em tb_acesso sem indicar se são entrada ou saída.
Por convenção, dentro de um mesmo dia os acessos se alternam:
1º = entrada, 2º = saída, 3º = entrada, 4º = saída, e assim por diante.

Os cálculos de permanência e de horas trabalhadas são feitos em memória
para funcionarem de forma idêntica em SQL Server, MySQL e SQLite.
*/
internal static class AcessoCalculos
{
    // retorna a duração de cada par entrada/saída ocorrido no mesmo dia
    // uma entrada sem saída correspondente (último acesso ímpar do dia) é ignorada
    public static IReadOnlyList<TimeSpan> CalcularPermanencias(IEnumerable<DateTime> acessos)
    {
        var permanencias = new List<TimeSpan>();

        foreach (var dia in acessos.OrderBy(a => a).GroupBy(a => a.Date))
        {
            var horarios = dia.ToList();

            for (int i = 0; i + 1 < horarios.Count; i += 2)
            {
                permanencias.Add(horarios[i + 1] - horarios[i]);
            }
        }

        return permanencias;
    }

    public static TimeSpan Somar(IEnumerable<TimeSpan> duracoes)
        => TimeSpan.FromTicks(duracoes.Sum(d => d.Ticks));

    public static TimeSpan Media(IReadOnlyCollection<TimeSpan> duracoes)
        => duracoes.Count == 0
            ? TimeSpan.Zero
            : TimeSpan.FromTicks((long)duracoes.Average(d => d.Ticks));

    // intervalo [início, fim) em DateTime a partir de datas opcionais
    // o fim é exclusivo no dia seguinte para incluir todos os acessos do dia final
    public static (DateTime? Inicio, DateTime? FimExclusivo) ConverterPeriodo(DateOnly? inicio, DateOnly? fim)
        => (inicio?.ToDateTime(TimeOnly.MinValue),
            fim?.AddDays(1).ToDateTime(TimeOnly.MinValue));
}
