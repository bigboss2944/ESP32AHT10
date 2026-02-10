# Collecteur de données ESP32 AHT10

Un système de collecte de données basé sur C#/.NET pour stocker et visualiser les données de température et d'humidité des capteurs ESP32 AHT10 avec SQLite et Grafana.

## Architecture

Cette solution respecte les **principes SOLID** et les modèles d'architecture propre :

### Projets

1. **DataCollector.Core** - Modèles de domaine et interfaces
   - Contient la logique métier et les entités du domaine
   - Définit les interfaces (ISensorReadingRepository, IDataParser, ISensorDataService)
   - Aucune dépendance externe

2. **DataCollector.Infrastructure** - Couche d'accès aux données
   - Implémente le pattern repository avec Entity Framework Core
   - Implémentation de la base de données SQLite
   - Dépend uniquement du projet Core

3. **DataCollector.Api** - Point d'entrée de l'application
   - Service d'écoute UDP en arrière-plan
   - Configuration de l'injection de dépendances
   - Hôte de service worker

4. **DataCollector.Tests** - Tests unitaires
   - Framework de test xUnit
   - Moq pour le mocking
   - FluentAssertions pour des assertions lisibles
   - **31 tests, tous réussis**

### Principes SOLID appliqués

- **Principe de responsabilité unique (SRP)** : Chaque classe a une seule responsabilité
  - `SensorDataParser` - analyse uniquement les données du capteur
  - `SensorDataService` - traite uniquement les données du capteur
  - `SensorReadingRepository` - gère uniquement la persistance des données

- **Principe ouvert/fermé (OCP)** : Ouvert à l'extension, fermé à la modification
  - Conception basée sur les interfaces permettant une extension facile
  - De nouveaux parsers ou repositories peuvent être ajoutés sans modifier le code existant

- **Principe de substitution de Liskov (LSP)** : Les classes dérivées sont substituables
  - Toutes les implémentations remplissent correctement leurs contrats d'interface

- **Principe de ségrégation des interfaces (ISP)** : Les clients ne dépendent que de ce dont ils ont besoin
  - Interfaces petites et ciblées (IDataParser, ISensorDataService, ISensorReadingRepository)

- **Principe d'inversion de dépendances (DIP)** : Dépendre des abstractions
  - Les modules de haut niveau (Api) dépendent des abstractions (Interfaces dans Core)
  - Les modules de bas niveau (Infrastructure) implémentent les abstractions

## Fonctionnalités

- ✅ Collecte de données UDP en temps réel depuis les capteurs ESP32
- ✅ Base de données SQLite pour le stockage persistant
- ✅ Tableau de bord Grafana pour la visualisation des données
- ✅ Conteneurisation Docker pour un déploiement facile
- ✅ Tests unitaires complets (31 tests)
- ✅ Principes de conception SOLID
- ✅ Architecture propre
- ✅ Injection de dépendances
- ✅ Patterns async/await
- ✅ Journalisation et gestion des erreurs

## Prérequis

- Docker et Docker Compose
- ESP32 avec capteur AHT10 (voir le répertoire `../ESP32AHT10` pour le firmware)
- .NET 8.0 SDK (pour le développement)

## Démarrage rapide

### 1. Construction et exécution avec Docker

```bash
cd DataCollector
docker-compose up -d
```

Cela démarrera :
- Service collecteur de données (écoute sur le port UDP 5000)
- Grafana (accessible à http://localhost:3000)

### 2. Accéder à Grafana

1. Ouvrez http://localhost:3000 dans votre navigateur
2. Connectez-vous avec :
   - Nom d'utilisateur : `admin`
   - Mot de passe : `admin`
3. Le "Sensor Data Dashboard" sera automatiquement provisionné

### 3. Configurer l'ESP32

Mettez à jour la configuration du firmware ESP32 pour envoyer des paquets UDP à l'adresse IP de votre Raspberry Pi sur le port 5000.

## Configuration

### Variables d'environnement

Configurez le service collecteur de données dans `docker-compose.yml` :

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Data Source=/data/sensordata.db
  - UdpListener__Port=5000
  - Logging__LogLevel__Default=Information
  - Logging__LogLevel__DataCollector=Debug
```

## Format des données

Le service attend des paquets UDP au format :
```
temp=25.50,hum=60.00
```

Exemple :
```
temp=22.30,hum=55.20
temp=-10.50,hum=100.00
```

## Schéma de base de données

### Table SensorReadings

| Colonne | Type | Description |
|---------|------|-------------|
| Id | INTEGER | Clé primaire (auto-incrémentation) |
| Temperature | REAL | Température en Celsius |
| Humidity | REAL | Pourcentage d'humidité (0-100) |
| Timestamp | TEXT | Horodatage UTC |
| DeviceId | TEXT | Identifiant du dispositif (adresse IP) |

## Développement

### Construction de la solution

```bash
cd DataCollector
dotnet restore
dotnet build
```

### Exécution des tests

```bash
dotnet test
```

Les 31 tests unitaires doivent réussir :
- ✅ Tests de parser (11 tests)
- ✅ Tests de service (10 tests)
- ✅ Tests de repository (10 tests)

### Exécution en local

```bash
cd DataCollector.Api
dotnet run
```

Le service écoutera les paquets UDP sur le port 5000.

## Test du service

### Envoyer des données de test

Vous pouvez tester le service avec netcat :

```bash
echo "temp=25.50,hum=60.00" | nc -u localhost 5000
echo "temp=22.30,hum=55.20" | nc -u localhost 5000
```

Ou avec PowerShell :
```powershell
$udpClient = New-Object System.Net.Sockets.UdpClient
$endpoint = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Parse("127.0.0.1"), 5000)
$bytes = [System.Text.Encoding]::ASCII.GetBytes("temp=25.50,hum=60.00")
$udpClient.Send($bytes, $bytes.Length, $endpoint)
$udpClient.Close()
```

## Tableau de bord Grafana

Le tableau de bord comprend :
- **Série temporelle de température** - Données historiques de température
- **Série temporelle d'humidité** - Données historiques d'humidité
- **Température actuelle** - Dernière lecture de température
- **Humidité actuelle** - Dernière lecture d'humidité
- **Total des lectures** - Nombre de toutes les lectures stockées

## Volumes et persistance des données

Les données sont persistées dans les volumes Docker :
- `sensor-data` - Base de données SQLite
- `grafana-storage` - Configuration et tableaux de bord Grafana

Pour sauvegarder la base de données :
```bash
docker cp sensor-datacollector:/data/sensordata.db ./backup.db
```

## Dépannage

### Vérifier les logs du service

```bash
docker logs sensor-datacollector
```

### Vérifier les logs Grafana

```bash
docker logs sensor-grafana
```

### Vérifier la base de données

```bash
docker exec -it sensor-datacollector sh
ls -la /data/
```

### Tester la connectivité UDP

Assurez-vous que le pare-feu autorise le port UDP 5000 :
```bash
# Linux
sudo ufw allow 5000/udp

# Vérifier si le service écoute
netstat -uln | grep 5000
```

## Licence

Ce projet fait partie du dépôt ESP32AHT10.

## Contribution

Les contributions sont les bienvenues ! Veuillez vous assurer que :
- Tous les tests unitaires réussissent
- Le code respecte les principes SOLID
- Les nouvelles fonctionnalités incluent des tests unitaires
- Le code est bien documenté
