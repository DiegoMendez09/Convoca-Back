namespace Backend.Services;

public static class MeritExamGenerator
{
    public const int MaxQuestions = 24;

    public static IReadOnlyList<ExamQuestionDraft> Build(MeritExamSource source)
    {
        var questions = new List<ExamQuestionDraft>();
        questions.AddRange(BehavioralQuestions(source.Level));

        var purpose = Clean(source.Purpose);
        if (purpose.Length >= 40)
            questions.Add(Choice(
                "Funcional",
                "Propósito del empleo",
                $"Según el manual de funciones de la OPEC {Label(source)}, el propósito principal del empleo es:",
                purpose,
                Fill(purpose, source, "Aprobar el presupuesto de la entidad.", "Nombrar el personal de libre nombramiento.", "Representar judicialmente a la entidad.")));

        foreach (var (line, index) in Lines(source.Functions).Take(6).Select((line, index) => (line, index)))
        {
            var others = Lines(source.Functions).Where(item => item != line).Take(3).ToArray();
            questions.Add(Choice(
                "Funcional",
                "Función esencial",
                $"¿Cuál de estas actuaciones es una función esencial de la OPEC {Label(source)}?",
                line,
                Fill(line, source, others.ElementAtOrDefault(0), others.ElementAtOrDefault(1), others.ElementAtOrDefault(2)),
                index + 1));
        }

        foreach (var (line, index) in Lines(source.Knowledge).Take(4).Select((line, index) => (line, index)))
        {
            var others = Lines(source.Knowledge).Where(item => item != line).Take(3).ToArray();
            questions.Add(Choice(
                "Conocimiento",
                "Conocimiento esencial",
                $"Un conocimiento esencial del manual de la OPEC {Label(source)} es:",
                line,
                Fill(line, source, others.ElementAtOrDefault(0) ?? "Topografía y diseño de vías.", others.ElementAtOrDefault(1) ?? "Contabilidad de costos industriales.", others.ElementAtOrDefault(2) ?? "Derecho penal especial."),
                index + 20));
        }

        var call = string.IsNullOrWhiteSpace(source.CallNumber) ? "la convocatoria" : source.CallNumber.Trim();
        foreach (var (line, index) in Lines(source.Axes).Take(4).Select((line, index) => (line, index)))
        {
            var others = Lines(source.Axes).Where(item => item != line).Take(3).ToArray();
            questions.Add(Choice(
                "Eje",
                "Eje de la convocatoria",
                $"La convocatoria {call} incluye este eje temático en la prueba:",
                line,
                Fill(line, source, others.ElementAtOrDefault(0) ?? "Mecánica de suelos.", others.ElementAtOrDefault(1) ?? "Farmacología clínica.", others.ElementAtOrDefault(2) ?? "Catastro y avalúos."),
                index + 40));
        }

        return questions.Take(MaxQuestions).ToArray();
    }

    public static string? NormalizeLevel(string? level)
    {
        var folded = Fold(level);
        return folded switch
        {
            "asistencial" => "Asistencial",
            "tecnico" => "Técnico",
            "profesional" => "Profesional",
            "asesor" => "Asesor",
            _ => null
        };
    }

    private static IEnumerable<ExamQuestionDraft> BehavioralQuestions(string level)
    {
        foreach (var item in CommonCompetencies.Concat(LevelCompetencies(level)))
        {
            yield return Choice(
                "Comportamental",
                item.Name,
                $"Situación de la competencia «{item.Name}», propia del nivel {level} o común a los empleos públicos. ¿Cuál conducta se ajusta a esa competencia?",
                item.Correct,
                [item.Wrong1, item.Wrong2, item.Wrong3],
                StableSeed(item.Name));
        }
    }

    private static IEnumerable<CompetencyItem> LevelCompetencies(string level)
    {
        return level switch
        {
            "Asistencial" => Asistencial,
            "Técnico" => Tecnico,
            "Asesor" => Asesor,
            _ => Profesional
        };
    }

    private static ExamQuestionDraft Choice(
        string section,
        string topic,
        string stem,
        string correct,
        IReadOnlyList<string?> distractors,
        int salt = 0)
    {
        var wrong = distractors
            .Select(Clean)
            .Where(item => item.Length > 0 && !item.Equals(correct, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();

        while (wrong.Count < 3)
            wrong.Add($"Actuación ajena al empleo {wrong.Count + 1}.");

        var options = new List<string> { Trim(correct) };
        options.AddRange(wrong.Select(Trim));
        Shuffle(options, StableSeed(correct) + salt);
        var correctIndex = options.FindIndex(option => option.Equals(Trim(correct), StringComparison.Ordinal));

        return new ExamQuestionDraft(section, topic, stem, options, correctIndex);
    }

    private static IReadOnlyList<string?> Fill(string correct, MeritExamSource source, params string?[] extras)
    {
        var pool = extras.Concat(Lines(source.Functions)).Concat(Lines(source.Knowledge)).ToArray();
        return pool;
    }

    private static IEnumerable<string> Lines(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        foreach (var raw in text.Split(['\n', '\r', ';'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim().TrimStart('-', '•', '*', '·').Trim();
            var dot = 0;
            while (dot < line.Length && (char.IsDigit(line[dot]) || line[dot] is '.' or ')'))
                dot++;
            if (dot > 0 && dot < line.Length)
                line = line[dot..].Trim();

            line = Clean(line);
            if (line.Length >= 12)
                yield return line;
        }
    }

    private static string Label(MeritExamSource source)
    {
        var code = string.IsNullOrWhiteSpace(source.OpecCode) ? "sin código" : source.OpecCode.Trim();
        var title = string.IsNullOrWhiteSpace(source.JobTitle) ? "el empleo" : source.JobTitle.Trim();
        return $"{code} ({title})";
    }

    private static string Clean(string? value)
    {
        return string.Join(' ', (value ?? string.Empty).Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries));
    }

    private static string Trim(string value)
    {
        var text = Clean(value);
        return text.Length <= 320 ? text : text[..320].Trim();
    }

    private static void Shuffle(List<string> options, int seed)
    {
        var random = new Random(seed);
        for (var index = options.Count - 1; index > 0; index--)
        {
            var swap = random.Next(index + 1);
            (options[index], options[swap]) = (options[swap], options[index]);
        }
    }

    private static int StableSeed(string text)
    {
        unchecked
        {
            var hash = 17;
            foreach (var character in text)
                hash = (hash * 31) + character;
            return hash;
        }
    }

    private static string Fold(string? value)
    {
        var text = (value ?? string.Empty).Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var chars = text.Where(character => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) != System.Globalization.UnicodeCategory.NonSpacingMark);
        return new string(chars.ToArray());
    }

    private static readonly CompetencyItem[] CommonCompetencies =
    [
        new("Aprendizaje continuo",
            "Busca cómo hacer mejor la tarea y aplica lo que acaba de aprender.",
            "Repite el mismo procedimiento aunque ya sepa que falla.",
            "Espera a que otro servidor le resuelva la duda y no la consulta.",
            "Guarda el material de capacitación sin usarlo en el trabajo."),
        new("Orientación al usuario y al ciudadano",
            "Explica el trámite con lenguaje claro y atiende primero la necesidad de la persona.",
            "Responde con el código interno y deja al ciudadano sin saber qué sigue.",
            "Atiende solo a quien insiste y deja las otras solicitudes sin respuesta.",
            "Pide datos que el trámite no requiere para alargar la atención."),
        new("Trabajo en equipo",
            "Comparte la información que el grupo necesita y cumple la parte que le corresponde.",
            "Guarda el avance para presentarlo solo al final.",
            "Cambia la tarea de un compañero sin acordarlo.",
            "Se retira del encargo cuando el resultado depende de otras áreas.")
    ];

    private static readonly CompetencyItem[] Asistencial =
    [
        new("Manejo de la información",
            "Custodia la información del empleo y solo transmite lo necesario, de forma oportuna.",
            "Comenta datos de una persona atendida con compañeros que no los necesitan.",
            "Deja documentos con datos personales sobre el escritorio compartido.",
            "Publica un listado interno en un chat personal."),
        new("Adaptación al cambio",
            "Ajusta la forma de hacer la tarea cuando cambia el procedimiento y pregunta lo que no entiende.",
            "Sigue el formato viejo porque el nuevo le quita tiempo.",
            "Espera a que devuelvan el proceso anterior para volver a trabajar.",
            "Rechaza la instrucción nueva sin leerla."),
        new("Disciplina",
            "Ejecuta la actividad como se la indicaron y acepta la revisión de su jefe.",
            "Cambia el orden del procedimiento porque le parece más rápido.",
            "Omite un paso de control para cerrar el turno.",
            "Entrega el resultado sin revisar si cumple la pauta."),
        new("Relaciones interpersonales",
            "Escucha el pedido y responde con un lenguaje claro, también a quien tiene una barrera de comunicación.",
            "Atiende de forma cortante a quien no entiende el trámite a la primera.",
            "Interrumpe al usuario para terminar más rápido.",
            "Trasladá la queja a otro puesto sin explicar el motivo."),
        new("Colaboración",
            "Articula su parte con la del compañero y cumple el compromiso que adquirió.",
            "Deja su cola de trabajo para que otro la cierre.",
            "Compite con el puesto de al lado por el mismo documento.",
            "Acepta el encargo y después no lo entrega.")
    ];

    private static readonly CompetencyItem[] Tecnico =
    [
        new("Confiabilidad técnica",
            "Aplica el conocimiento de su oficio al caso y deja el concepto claro y ajustado a la pauta.",
            "Da un concepto general sin revisar la norma del procedimiento.",
            "Copia el informe anterior aunque el caso sea distinto.",
            "Entrega el concepto sin unidades ni criterio de calidad."),
        new("Disciplina",
            "Sigue la instrucción y contrasta el resultado con el proceso vigente.",
            "Improvisa un atajo que el proceso no autoriza.",
            "Omite el registro porque la tarea ya quedó hecha.",
            "Cambia la plantilla institucional por una personal."),
        new("Responsabilidad",
            "Usa el tiempo y los elementos del empleo para terminar la tarea con calidad.",
            "Deja el equipo desordenado y la tarea a medias.",
            "Culpa al turno anterior sin revisar su propia entrega.",
            "Cierra el caso fuera del plazo sin avisar el atraso."),
        new("Manejo de la información",
            "Organiza la información técnica y no la comparte fuera de quien debe usarla.",
            "Envía el archivo de trabajo a un correo personal.",
            "Mezcla expedientes de dos casos en la misma carpeta.",
            "Borra el soporte porque el informe ya se entregó."),
        new("Adaptación al cambio",
            "Actualiza su forma de ejecutar cuando cambia el instructivo técnico.",
            "Mantiene la herramienta antigua porque ya la domina.",
            "Pide que el cambio espere hasta el otro año.",
            "Aplica la mitad del instructivo nuevo y la mitad del viejo.")
    ];

    private static readonly CompetencyItem[] Profesional =
    [
        new("Aporte técnico-profesional",
            "Pone su saber profesional al servicio del caso y anticipa el problema que ya se puede prever.",
            "Deja el concepto en manos de otra profesión para no quedar comprometido.",
            "Repite la orden recibida aunque choque con el conocimiento del caso.",
            "Oculta la experiencia previa para que el grupo no dependa de su criterio."),
        new("Comunicación efectiva",
            "Escribe el informe con precisión y responde lo que le preguntan después de leerlo completo.",
            "Responde el correo con una frase ambigua y sin el dato pedido.",
            "Usa siglas internas en un mensaje dirigido al ciudadano.",
            "Interrumpe la explicación del otro para imponer la conclusión."),
        new("Gestión de procedimientos",
            "Ejecuta la tarea con el criterio de calidad del procedimiento y propone un ajuste concreto de tiempo.",
            "Salta el control porque el resultado se ve completo.",
            "Crea un trámite paralelo que la entidad no tiene.",
            "Guarda el cuello de botella para explicarlo al final del año."),
        new("Instrumentación de decisiones",
            "Decide lo que le corresponde con información válida y asume el efecto de esa decisión.",
            "Eleva al jefe una decisión que el manual deja en su puesto.",
            "Elige la opción más rápida sin mirar el costo ni la evidencia.",
            "Atribuye la decisión al equipo cuando fue solo suya."),
        new("Orientación a resultados",
            "Ordena el trabajo hacia el producto que el empleo debe entregar y corrige lo que lo desvía.",
            "Multiplica reuniones sin un entregable.",
            "Mide el avance por las horas ocupadas y no por el producto.",
            "Cierra la meta con un informe que no responde el encargo.")
    ];

    private static readonly CompetencyItem[] Asesor =
    [
        new("Confiabilidad técnica",
            "Sustenta la asesoría con el conocimiento del tema y la ajusta a los lineamientos de la entidad.",
            "Emite un concepto oral sin dejar el criterio por escrito.",
            "Aconseja según la costumbre del área y no según la norma.",
            "Trasladá la pregunta a un subordinado y firma el texto sin leerlo."),
        new("Creatividad e innovación",
            "Propone una alternativa nueva cuando la vía habitual no resuelve el problema.",
            "Descarta toda idea que no esté en el formato anterior.",
            "Cambia el procedimiento por novedad, sin un resultado esperado.",
            "Espera una consultoría externa para cualquier ajuste."),
        new("Iniciativa",
            "Anticipa el problema y avanza la tarea que ya está en su competencia.",
            "Espera la instrucción escrita para un asunto que ya conoce y le corresponde.",
            "Abre frentes que no pidieron y abandona el encargo principal.",
            "Señala el riesgo y no propone qué hacer."),
        new("Construcción de relaciones",
            "Mantiene el diálogo con las áreas que necesita la asesoría y respeta el desacuerdo.",
            "Impone el concepto sin oír al área responsable del proceso.",
            "Acuerda por fuera del expediente para evitar el debate.",
            "Limita la conversación a su grupo inmediato."),
        new("Conocimiento del entorno",
            "Lee el contexto de la entidad y lo usa al formular el concepto.",
            "Asesora como si la entidad no tuviera restricciones legales ni políticas.",
            "Copia un modelo de otra entidad sin mirar la misión propia.",
            "Ignora una demanda ciudadana porque no está en el último informe.")
    ];

    private sealed record CompetencyItem(string Name, string Correct, string Wrong1, string Wrong2, string Wrong3);
}

public sealed record MeritExamSource(
    string Level,
    string? CallNumber,
    string? OpecCode,
    string? JobTitle,
    string? Purpose,
    string? Functions,
    string? Knowledge,
    string? Axes);

public sealed record ExamQuestionDraft(
    string Section,
    string Topic,
    string Stem,
    IReadOnlyList<string> Options,
    int CorrectIndex);
