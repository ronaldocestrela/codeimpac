# Configurações do GitHub no CodeImpact

Este documento explica as configurações da seção `GitHub` em `src/backend/CodeImpact.WebApi/appsettings.json` e como obter cada valor.

## Onde essas configurações ficam

No arquivo `src/backend/CodeImpact.WebApi/appsettings.json`, a seção atual está assim:

```json
"GitHub": {
  "ClientId": "YourGitHubClientId",
  "ClientSecret": "YourGitHubClientSecret",
  "RedirectUri": "https://localhost:5173/github/callback",
  "Scope": "read:user repo"
}
```

## O que significa cada campo

- `ClientId`: identificador público da aplicação OAuth criada no GitHub.
- `ClientSecret`: segredo privado da aplicação OAuth (nunca deve ser exposto em repositório ou frontend).
- `RedirectUri`: URL para a qual o GitHub redireciona o usuário após o login/autorização.
- `Scope`: permissões solicitadas para acessar dados da conta e dos repositórios.

## Como criar as credenciais no GitHub

1. Acesse: `https://github.com/settings/developers`
2. Clique em **OAuth Apps**.
3. Clique em **New OAuth App**.
4. Preencha os campos:
   - **Application name**: por exemplo, `CodeImpact Local`.
   - **Homepage URL**: por exemplo, `http://localhost:5173`.
   - **Authorization callback URL**: deve ser igual ao `RedirectUri` usado na API.
     Exemplo: `https://localhost:5173/github/callback`.
5. Clique em **Register application**.
6. Copie o **Client ID** e cole em `ClientId`.
7. Clique em **Generate a new client secret**, copie o valor e cole em `ClientSecret`.

## Como definir o RedirectUri corretamente

O valor de `RedirectUri` deve corresponder exatamente ao callback configurado no GitHub (incluindo protocolo, porta e path).

Exemplos:
- Ambiente local frontend: `http://localhost:5173/github/callback` ou `https://localhost:5173/github/callback`
- Produção: `https://seu-dominio.com/github/callback`

Se houver diferença, o GitHub retorna erro de `redirect_uri_mismatch`.

## Escopos recomendados

Escopos atuais no projeto:
- `read:user`: leitura básica de dados do usuário autenticado.
- `repo`: acesso a repositórios privados e públicos (escopo amplo).

Observação importante:
- Se possível, aplique princípio de menor privilégio e reduza permissões quando não precisar de acesso completo ao `repo`.

## Exemplo de configuração (desenvolvimento)

```json
"GitHub": {
  "ClientId": "Iv1.xxxxxxxxxxxxx",
  "ClientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "RedirectUri": "http://localhost:5173/github/callback",
  "Scope": "read:user repo"
}
```

## Boas práticas de segurança

- Nunca commitar `ClientSecret` em repositório.
- Para ambiente local, prefira usar User Secrets do .NET:

```bash
dotnet user-secrets set "GitHub:ClientId" "SEU_CLIENT_ID" --project src/backend/CodeImpact.WebApi/CodeImpact.WebApi.csproj
dotnet user-secrets set "GitHub:ClientSecret" "SEU_CLIENT_SECRET" --project src/backend/CodeImpact.WebApi/CodeImpact.WebApi.csproj
```

- Em produção, use variáveis de ambiente ou serviço de segredos (ex.: Azure Key Vault).

## Checklist rápido

- OAuth App criada no GitHub.
- `ClientId` e `ClientSecret` preenchidos corretamente.
- `RedirectUri` igual ao callback cadastrado no GitHub.
- `Scope` compatível com as necessidades do sistema.
- Segredos fora de arquivos versionados.
