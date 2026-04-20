# Backoffice & Billing Implementation Plan

## Objetivo
Implementar o módulo administrativo (backoffice) e o fluxo de cobrança por assinatura para transformar a aplicação em um SaaS B2C operável em produção.

## Escopo MVP

### Backoffice
- Autenticação e autorização para perfis administrativos (`Owner`, `Admin`, `Manager`, `Viewer`).
- Gestão de usuários finais: status da conta, bloqueio/desbloqueio e histórico de suporte.
- Gestão de assinaturas: plano atual, status de cobrança e ações de suporte.
- Operação de jobs: listar, filtrar, reprocessar (`sync`, `AI summary`, `report`).
- Auditoria: registrar ações sensíveis (quem, quando, o que, payload resumido).

### Billing
- Catálogo de planos (mensal/anual) com limites de uso.
- Checkout hospedado por provedor (recomendado: Stripe).
- Portal de assinatura (upgrade/downgrade, método de pagamento, invoices).
- Webhooks com idempotência para ciclo de vida de assinatura.
- Bloqueio progressivo por limite excedido e status de assinatura.

## Modelo de domínio sugerido

### Novas entidades
- `UserSubscription`
- `Subscription`
- `BillingCustomer`
- `Invoice`
- `PaymentEvent`
- `Plan`
- `UserUsageSnapshot`
- `SupportTicket`
- `AdminAuditLog`

### Regras essenciais
- Todo `User` deve ter contexto de plano (`free`, `pro`, `premium`) para habilitação de recursos.
- Toda execução de relatório/ingestão deve validar entitlement do usuário.
- Entitlements devem ser avaliados antes de iniciar jobs custosos.
- Eventos de webhook devem ser idempotentes por `ProviderEventId`.

## API (alto nível)

### Backoffice
- `GET /api/admin/users`
- `GET /api/admin/users/{userId}`
- `PATCH /api/admin/users/{userId}/status`
- `GET /api/admin/users/{userId}/subscription`
- `PATCH /api/admin/users/{userId}/subscription`
- `GET /api/admin/jobs`
- `POST /api/admin/jobs/{jobId}/retry`
- `GET /api/admin/audit-logs`

### Billing
- `GET /api/billing/plans`
- `POST /api/billing/checkout-session`
- `GET /api/billing/subscription`
- `POST /api/billing/portal-session`
- `GET /api/billing/invoices`
- `POST /api/billing/webhook` (public endpoint with signature validation)

## Frontend (alto nível)

### Rotas
- `/admin`
- `/admin/users`
- `/admin/subscriptions`
- `/admin/jobs`
- `/admin/audit`
- `/settings/billing`

### Componentes-chave
- Tabelas administrativas com filtros por plano/status/periodo.
- Drawer/modal para ações operacionais (retry/revoke).
- Painel de status de assinatura e consumo do plano.
- Alertas de limite e bloqueio de funcionalidades premium.

## Sequência recomendada de implementação
1. Criar modelo de assinatura por usuário (`UserSubscription`, `Plan`, `UserUsageSnapshot`).
2. Implementar RBAC e proteger rotas/admin endpoints.
3. Entregar listagem de jobs + retry + auditoria.
4. Integrar checkout + webhook + persistência de subscription.
5. Aplicar entitlements nos fluxos críticos (sync/report/export).
6. Entregar páginas de billing e administração no frontend.

## Testes obrigatórios
- Unit tests para regras de RBAC e entitlements.
- Integration tests para endpoints administrativos.
- Integration tests para webhook (assinatura válida/inválida, idempotência, reprocesso).
- Testes de regressão para garantir que usuários fora do plano não acessem recursos pagos.

## Observabilidade mínima
- Métricas de webhook: recebidos, processados, falhos, duplicados.
- Métricas de entitlement: bloqueios por limite/plano.
- Dashboard de jobs por usuário com taxa de erro.
- Alertas para `past_due`, falhas de cobrança e backlog de reprocessamento.

## Critérios de aceite MVP
- Admin consegue operar usuários/assinaturas/jobs sem acesso ao banco.
- Checkout cria assinatura e atualiza status local por webhook.
- Limites de plano são aplicados de forma consistente no backend.
- Usuário final visualiza status de plano, invoices e ações de upgrade.
- Ações administrativas e eventos de cobrança são auditáveis ponta a ponta.
