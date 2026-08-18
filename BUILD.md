# Build deterministica

Il workflow compila esclusivamente il progetto `Windows11MaintenanceCenter.csproj` della root.

Il workflow:
1. restore
2. publish self-contained x64
3. verifica dell'esistenza dell'EXE
4. upload artifact

Il fatto che nella repository rimangano vecchi file da tentativi precedenti non deve alterare la compilazione,
perché il progetto disabilita gli elementi di compilazione predefiniti e include esplicitamente solo i file canonici.
