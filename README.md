# BotSaaS

Plataforma SaaS de atendimento por IA no WhatsApp para pequenos negócios. O bot é
**transacional**: o cliente final faz um **pedido** ou **marca um horário** pela conversa,
e isso vira um **registro estruturado** que o dono do negócio acompanha num painel.

> Projeto de portfólio em desenvolvimento — foco em arquitetura limpa e integração de IA
> ponta a ponta. Construído incrementalmente, com cada decisão documentada.

## Stack

| Camada | Escolha |
|---|---|
| Backend | C# .NET 10, ASP.NET Core Web API |
| Banco | MySQL |
| Acesso a dados | **ADO.NET puro** (sem ORM/Dapper) + driver MySqlConnector |
| Auth | JWT próprio (HMAC-SHA256) + BCrypt |
| IA | Google Gemini ou Groq/Llama (atrás de interface neutra `IAiClient`, trocável) |

## Arquitetura

Monólito modular com dependência em mão única:

```
Modules/   → nichos/verticais que estendem o Core (ex: Barber)
   ↓
Core/      → o núcleo do produto (Auth, Users, Companies, Conversations, AI...)
   ↓
Shared/    → base técnica neutra (Results, Security, Database, AI)
```

Decisões de design que guiam o código:

- **Multi-tenant** por `CompanyId`, isolado via JWT (o tenant sai sempre do token).
- **Repositório "burro"** — só executa SQL ou propaga a exceção do driver; tradução de
  erro fica nas camadas de cima (Service → código semântico → Controller → HTTP).
- **Transação explícita** via `DbSession` compartilhado na requisição (registro de
  Company + User é atômico: tudo ou nada).
- **Interfaces neutras** escondem a tecnologia — `IDatabase`, `IPasswordHasher`,
  `IJwtTokenGenerator` e o cliente de IA. Trocar de provedor é trocar uma implementação.
- **Result Pattern** para fluxo de erro sem exceções de controle.

## Funcionalidades atuais

- ✅ Cadastro de empresa + usuário (transacional)
- ✅ Login com verificação BCrypt e emissão de JWT
- ✅ Rota protegida lendo a identidade/tenant das claims do token
- ✅ Integração com IA respondendo via API — dois provedores trocáveis por DI (Gemini e Groq/Llama)
- ✅ Conversas com histórico persistido no banco (memória por cliente) — testado e2e
- 🔨 Extração de ação via **function calling** (tool calling) — o modelo extrai um `Agendamento` estruturado da conversa; recebimento provado e2e, domínio em construção
- 🔜 Canal de WhatsApp, painel do dono, cobrança

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/auth/register` | Cria empresa + usuário |
| `POST` | `/api/auth/login` | Autentica e devolve o JWT |
| `GET`  | `/api/me` | Identidade do requisitante (requer Bearer) |
| `POST` | `/api/conversations` | Processa uma mensagem do cliente e devolve a resposta da IA (requer Bearer; entrada dev/token — produção será o webhook) |

## Como rodar

**Pré-requisitos:** SDK do .NET 10, MySQL (ex: XAMPP), uma chave de API de IA (Google Gemini ou Groq).

1. Clone o repositório e crie um arquivo `.env` na raiz.
2. Preencha o `.env` com suas variáveis: banco (`DB_HOST/USER/PASSWORD/NAME/PORT`),
   JWT (`JWT_SECRET/ISSUER/AUDIENCE/EXPIRES_HOURS`), IA (`GEMINI_API_KEY/MODEL` e/ou
   `GROQ_API_KEY/MODEL`) e `SYSTEM_PROMPT`. O provedor ativo é escolhido na DI (`Program.cs`).
3. Crie o banco e as tabelas `companies`, `users`, `conversations` e `messages` no MySQL.
4. Rode:
   ```bash
   dotnet run
   ```

A API sobe em `http://localhost:5069`.

## Estrutura

```
src/
├── Core/
│   ├── Auth/          registro e login (orquestra o cadastro)
│   ├── Users/         entidade User + gestão
│   ├── Companies/     os tenants (empresas clientes)
│   └── Conversations/ conversa + mensagens (Conversation/Message + repositórios)
└── Shared/
    ├── Results/     Result Pattern
    ├── Security/    hash de senha + JWT
    ├── Database/    conexão e sessão de transação
    └── AI/          cliente de IA (Gemini e Groq, atrás de IAiClient)
```
