# BotSaaS — Painel Web (Frontend)

Painel administrativo e operacional para donos e recepcionistas, construído em **Angular 21** (Standalone Components, Signals, Zoneless).

## Funcionalidades da Interface

- **Grade de Cadeiras Executiva (`/appointments`):**
  - Visualização de horários ($08:00$ às $19:00$) $\times$ profissionais em colunas.
  - Suporte a múltiplos agendamentos no mesmo slot de hora sem sobreposição.
  - Alternância fluida para **Lista Cronológica**.
  - Navegador de datas (`‹ Hoje, DD Mês ›`) com avanço e recuo diário.
  - Dropdowns executivos customizados para filtro por Profissional e Status.
  - Modal de detalhes do agendamento com histórico de status e link direto para a conversa com o cliente.
- **Gestão de Equipe (`/team`):**
  - Cadastro, edição inline e alternância de status de profissionais (`Ativo`, `Pausa/Férias`, `Inativo`).
- **Configurações & Horários de Funcionamento (`/settings`):**
  - Definição da grade semanal com abertura, fechamento e dias fechados, com cópia rápida para dias úteis.
- **Histórico da Conversa (`/conversations/:id`):**
  - Visualização da conversa de chat que originou o agendamento.
- **Autenticação (`/login`):**
  - Login seguro via JWT com persistência e interceptor HTTP automático (`Bearer`).

## Execução em Desenvolvimento

```bash
npm install
npm start # ou ng serve
```

Acesse em `http://localhost:4200`. A API backend (.NET) deve estar rodando na porta 5069.

## Build de Produção

```bash
npm run build
```
Os artefatos compilados serão gerados no diretório `dist/web`.
