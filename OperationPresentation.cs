using Windows11MaintenanceCenter.Core;

namespace Windows11MaintenanceCenter;

public sealed record OperationPresentation(string StateLabel, string Title, string Summary, string Detail);

public static class OperationPresentationHelper
{
    public static OperationPresentation Create(CommandResult r)
    {
        if (r.TimedOut)
            return new("NEGATIVO", "Tempo massimo superato",
                "Windows non ha completato l'operazione nel tempo previsto.",
                "Il programma non trae conclusioni sullo stato di Windows dal solo timeout.");

        if (r.Operation == "Pending reboot")
        {
            return r.Output.Contains("REBOOT_REQUIRED", StringComparison.OrdinalIgnoreCase)
                ? new("RIAVVIO NECESSARIO", "Riavvio necessario",
                    "Windows segnala che ci sono operazioni pendenti.",
                    "Il programma non riavvia automaticamente il computer.")
                : new("POSITIVO", "Nessun riavvio pendente",
                    "Non sono stati rilevati indicatori di riavvio necessario.",
                    "Controllo eseguito in sola lettura.");
        }

        if (r.Operation == "Service state")
            return r.Output.Contains("NOT_PRESENT", StringComparison.OrdinalIgnoreCase)
                ? new("ATTENZIONE / PARZIALE", "Controllo servizi completato",
                    "Uno o più servizi previsti non risultano presenti.",
                    "Questo può essere normale in base all'edizione o alle funzioni installate.")
                : new("POSITIVO", "Servizi verificati",
                    "Lo stato dei servizi selezionati è stato letto correttamente.",
                    "Nessun servizio è stato avviato, arrestato o modificato.");

        if (r.Operation == "Update provider inventory")
            return r.Output.Contains("NOT_INSTALLED", StringComparison.OrdinalIgnoreCase)
                ? new("POSITIVO", "Rilevamento completato",
                    "Alcuni strumenti di aggiornamento non sono installati.",
                    "Nessun componente è stato installato automaticamente.")
                : new("POSITIVO", "Strumenti rilevati",
                    "La disponibilità dei gestori di aggiornamento è stata verificata.",
                    "Nessun gestore è stato modificato.");

        if (r.Operation == "SYSTEM hive load test")
            return r.Output.Contains("CRITICAL", StringComparison.OrdinalIgnoreCase)
                ? new("NEGATIVO", "Test dell'hive non completato",
                    "Il programma ha rilevato un problema nello scaricamento dell'hive temporaneo.",
                    "Non vengono eseguiti ulteriori test automatici.")
                : new("POSITIVO", "Test dell'hive completato",
                    "Il caricamento e lo scaricamento controllato dell'hive SYSTEM sono terminati.",
                    "Il test non è progettato per modificare permanentemente il Registro.");

        if (r.Operation == "SFC /verifyonly")
            return r.ExitCode == 0
                ? new("POSITIVO", "Verifica file di sistema completata",
                    "SFC ha terminato la verifica senza errore del processo.",
                    "È stata usata la modalità /verifyonly: nessun file viene riparato automaticamente.")
                : new("ATTENZIONE / PARZIALE", "Verifica SFC con anomalie",
                    "SFC ha restituito un risultato che richiede attenzione.",
                    "Il programma non esegue automaticamente una riparazione.");

        if (r.ExitCode == 0)
            return new("POSITIVO", "Operazione completata",
                "Windows ha completato l'operazione senza errore del processo.",
                "Nessuna riparazione automatica è stata avviata.");

        return new("NEGATIVO", "Operazione non completata",
            "Windows ha restituito un errore durante l'operazione.",
            "Apri i dettagli tecnici per vedere il risultato originale.");
    }
}
