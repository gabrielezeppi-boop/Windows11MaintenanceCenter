# Windows 11 Maintenance Center — Full Release

Questo ZIP contiene l'intero progetto aggiornato.

## Build
Il workflow GitHub è in:
`.github/workflows/build.yml`

Il progetto viene compilato esclusivamente da:
`Windows11MaintenanceCenter.csproj`

Grazie alle proprietà `EnableDefaultCompileItems=false`, `EnableDefaultPageItems=false` e
`EnableDefaultApplicationDefinition=false`, eventuali vecchi file rimasti nella repository
non vengono inclusi nella compilazione.

## Funzioni conservative
- Informazioni sistema
- DISM /CheckHealth
- DISM /ScanHealth
- SFC /verifyonly
- CHKDSK /scan
- lettura servizi
- rilevazione riavvio pendente
- inventario Registry/hive
- Shadow Copy inventory
- test controllato SYSTEM hive
- Windows Update e WinGet solo con conferma

Sono escluse riparazioni automatiche e operazioni invasive.
