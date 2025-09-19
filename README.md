🔁 Step-by-step per ottenere ruoli con privilegi su lead
1. Recupera i privilegi disponibili per l'entità lead
http
GET [YourOrgUrl]/api/data/v9.2/EntityDefinitions(LogicalName='lead')/Privileges
Questo ti restituisce una lista di oggetti Privilege, ciascuno con:

PrivilegeId

Name (es. prvReadLead, prvWriteLead, ecc.)

AccessRight (Read, Write, Create, ecc.)

2. Interroga RolePrivileges per ciascun PrivilegeId
Per ogni PrivilegeId ottenuto, fai:

http
GET [YourOrgUrl]/api/data/v9.2/RolePrivileges?$filter=PrivilegeId eq [GUID]
Questo ti restituisce i RoleId che hanno quel privilegio.

3. Recupera i nomi dei ruoli
Per ogni RoleId, fai:

http
GET [YourOrgUrl]/api/data/v9.2/Roles([RoleId])
🧠 Privilegi da cercare per lead
Privilegio	Nome API tipico
Read	prvReadLead
Write	prvWriteLead
Create	prvCreateLead
Delete	prvDeleteLead
Assign	prvAssignLead
Share	prvShareLead
AssignTo	(incluso in Assign)
🧰 Alternativa più semplice: XrmToolBox
Se vuoi evitare chiamate manuali, usa il plugin Privileges Discovery o Access Security Roles in XrmToolBox. Ti permette di:

4. Per ogni ruolo trovato cerca gli egl_userprofile che sono associati tramite il campo string multilinea egl_roles

### Output atteso

| Ruolo   | Privilegi                  | UserProfiles |
|---------|----------------------------|--------------|
| RUOLO1  | READ ORD; WRITE USER       | UP1, UP2     |

Il comando pacx mostrerà una tabella con:
- Nome ruolo
- Privilegi associati (nome + livello)
- UserProfile che hanno il ruolo nel campo egl_roles


Quindi per fare un esempio vorrei cercare i ruoli privilegi e userprofile dell'entità lead

lead ha un solo ruolo RUOLO1
RUOLO1 ha il privilegio di READ a livello organization e il WRITE a livello USER
gli userprofile UP1 e UP2 nel campo egl_roles hanno tra i vari ruoli anche il RUOLO1

pertanto se io ricerco per entità lead mi aspetto di ricevere:
- RUOLO1 -> READ ORD; WRITE USER -> UP1 UP2