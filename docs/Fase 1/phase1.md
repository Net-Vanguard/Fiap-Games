 [📘 Documentação Técnica da Fase 1](https://github.com/leandrokuranaga/fiap-back/wiki/Documentação-Técnica-da-fase-1)

## 📦 Sobre o Projeto

O **FCG** é uma aplicação backend em **.NET 8**, com o objetivo de simular uma plataforma de venda e gerenciamento de jogos digitais. Esta é a **primeira entrega**, focada em:

- Autenticação de usuários  
- Registro de jogos  
- Promoções  
- Controle da biblioteca pessoal

---

## 🛠️ Pré-requisitos

- Docker
- Docker Compose  
- .NET SDK 8.0  
- PostgreSQL (caso não use o Docker)

---

## 📚 Links Úteis

- [📘 .NET 8](https://learn.microsoft.com/en-us/dotnet/)
- [📘 EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [📘 Docker](https://docs.docker.com/)

---

### 📌 Seed Inicial (EF Core `HasData()`)

O projeto utiliza **`HasData()` com EF Core** para inserir dados iniciais automaticamente ao aplicar as migrations. Isso facilita testes e demonstrações, evitando a necessidade de popular o banco manualmente.

Inclui:

- Usuários padrão (Admin, User)
- Jogos (8 títulos)
- Promoções (3)
- Bibliotecas de jogos por usuário

#### Dados incluídos:
- **Usuários:**
  - `Admin` (admin@gmail.com)
  - `User` (user@gmail.com)
- **Bibliotecas:**
  - Uma biblioteca associada a cada usuário
- **Jogos:**
  - 8 títulos inseridos com nome, gênero e preço
- **Promoções:**
  - 3 promoções com datas de início e fim
- **LibraryGames:**
  - Jogos comprados por usuários com preço pago e data de compra


# Credenciais de Acesso

## Usuário Comum  
- **Login:** `user@domain.com`  
- **Senha:** `F1ap@Senha`

## Usuário Admin  
- **Login:** `admin@domain.com`  
- **Senha:** `F1ap@Senha`

---

# 🧪 Guia para rodar o projeto com Docker e SonarQube

Este projeto depende de **Docker** para executar seus serviços (API, banco de dados e SonarQube).

---

## 🐳 Subindo os serviços com Docker

```bash
docker compose up -d --build
```

Acesse a API: [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)

## 🎯 Subir serviços específicos:

```bash
docker compose up -d db sonarqube
docker compose up -d app
```

Você também pode iniciar um serviço por vez:

```bash
docker compose up -d db          # Banco de dados
docker compose up -d app         # API
docker compose up -d sonarqube   # SonarQube
```

---

## 🔐 Configuração do SonarQube

1. Acesse: [http://localhost:9000](http://localhost:9000)  
2. Login:  
  - **Usuário:** `admin`
   - **Senha:** `admin`
3. Altere a senha, gere um token em **My Account > Security**
4. No arquivo `.env-dev`, adicione:

```env
SONAR_TOKEN=seu_token_gerado
```

5. Renomeie o arquivo:

```bash
mv .env-dev .env
```

## ▶️ Análise com SonarQube

1. Execute `sonar-analyze.bat`
2. Veja os resultados em [http://localhost:9000](http://localhost:9000)
3. Vá até a seção "Projetos" para visualizar os resultados da análise
---

## 🛢️ Configuração do banco de dados

No arquivo `.env` (anteriormente `.env-dev`), configure as variáveis:

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=sua_senha
POSTGRES_DB=FIAP
```

### Configurar o ambiente (`appsettings.json`) (`appsettings.Development.json`)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=db;Database=FIAP;User Id=seu_user;Password=sua_senha;"
}
```

Essas variáveis são usadas pelo Docker para inicializar o banco e pela aplicação para se conectar a ele.

---

# 🖥️ Execução Manual (sem Docker)

### 1. Clonar o projeto

```bash
git clone https://github.com/leandrokuranaga/fiap-back.git
cd fiap-back
```

### 2. Restaurar pacotes

```bash
dotnet restore
```

### 3. Configurar o banco (`appsettings.json`) (`appsettings.Development.json`)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FIAP;User Id=seu_user;Password=sua_senha;"
}
```

### 4. Rodar a aplicação

```bash
dotnet run --project ./src/Fiap.Api
```

---
