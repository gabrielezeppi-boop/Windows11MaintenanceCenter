namespace Windows11MaintenanceCenter;

public sealed record OperationInfo(string Title, string WhatItDoes, string Safety, string Estimate);

public static class OperationCatalog
{
    public static OperationInfo Get(string operation) => operation switch
    {
        "System information" => new("Informazioni del PC",
            "Legge versione di Windows, build, architettura, ultimo avvio e spazio libero.",
            "Sola lettura. Non modifica nulla.", "Pochi secondi"),

        "DISM CheckHealth" => new("Controllo rapido di Windows",
            "Controlla se Windows ha già segnalato una possibile corruzione dell'immagine.",
            "Diagnostica. Nessuna riparazione.", "10–30 secondi"),

        "DISM ScanHealth" => new("Analisi approfondita di Windows",
            "Analizza il component store di Windows alla ricerca di corruzione.",
            "Diagnostica. Nessuna riparazione.", "5–20 minuti"),

        "SFC /verifyonly" => new("Verifica dei file di sistema",
            "Controlla i file di sistema protetti senza sostituirli e senza ripararli.",
            "Sola verifica. Nessuna riparazione.", "5–15 minuti"),

        "CHKDSK /scan" => new("Controllo del disco",
            "Controlla online il file system del disco di sistema.",
            "Solo diagnostica. Non usa /F, /R o /X.", "1–15 minuti"),

        "Service state" => new("Controllo dei servizi essenziali",
            "Legge lo stato dei principali servizi Windows scelti dal programma.",
            "Sola lettura. Nessun servizio viene modificato.", "Pochi secondi"),

        "Pending reboot" => new("Controllo riavvio necessario",
            "Verifica se Windows ha operazioni pendenti che richiedono un riavvio.",
            "Sola lettura. Il programma non riavvia il PC.", "Pochi secondi"),

        "Registry inventory" => new("Informazioni del Registro",
            "Legge solo informazioni Windows e la directory RegBack.",
            "Sola lettura. Nessuna chiave viene modificata.", "Pochi secondi"),

        "Registry hive inventory" => new("File del Registro",
            "Controlla presenza e metadati degli hive principali.",
            "Sola lettura.", "Pochi secondi"),

        "Shadow Copy inventory" => new("Copie Shadow",
            "Elenca le copie shadow disponibili.",
            "Sola lettura. Nessuna copia viene cancellata.", "Pochi secondi"),

        "SYSTEM hive load test" => new("Test dell'hive SYSTEM",
            "Carica temporaneamente l'hive SYSTEM e verifica che venga scaricato correttamente.",
            "Test controllato. Nessuna modifica permanente prevista.", "Pochi secondi"),

        "Update provider inventory" => new("Strumenti di aggiornamento",
            "Controlla quali gestori di aggiornamenti sono già presenti.",
            "Sola lettura. Non installa nulla.", "Pochi secondi"),

        "WinGet user-approved upgrade" => new("Aggiornamento applicazioni",
            "Cerca e installa aggiornamenti delle applicazioni gestite da WinGet.",
            "Modifica le applicazioni. Richiede conferma.", "2–20 minuti"),

        "Windows Update user-approved" => new("Aggiornamento Windows",
            "Cerca e installa gli aggiornamenti disponibili tramite Windows Update.",
            "Può modificare Windows e richiedere un riavvio.", "5–45 minuti"),

        _ => new(operation, "Esegue una verifica di sistema.", "Controlla prima di procedere.", "Variabile")
    };
}
