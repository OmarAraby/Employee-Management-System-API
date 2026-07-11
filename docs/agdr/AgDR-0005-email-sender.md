# Email sending — System.Net.Mail + smtp4dev catcher for dev, config-driven relay for prod

> In the context of adding self-service password reset (ticket #21), which needs to send email — and the app had no email infrastructure — I decided to introduce a small `IEmailSender` abstraction implemented with the built-in `System.Net.Mail.SmtpClient`, backed in local dev by an smtp4dev catcher container and in prod by a config-driven SMTP relay, to achieve working email with zero new NuGet dependencies and no real mail sent during development, accepting that a production-grade provider will want MailKit or a transactional-email API instead.

## Context

- Forgot-password (#21) requires emailing a reset link. The codebase had **no** email sender, SMTP config, or mail dependency.
- Local dev must not send real email, and shouldn't require a developer to own an SMTP account just to test the flow.
- The sender is consumed by the Auth manager (BL), so the abstraction belongs consumer-side; the transport implementation belongs in the API/infra layer (mirrors the existing `IFileService`/`FileService` split).

## Options Considered

| Axis | Options | Choice |
|------|---------|--------|
| **SMTP client** | (a) `System.Net.Mail.SmtpClient` (built-in); (b) MailKit (MS-recommended); (c) a transactional API (SendGrid/SES SDK) | **(a)** — zero dependency, adequate for smtp4dev + a plain relay. MailKit/SendGrid recorded as the prod-hardening upgrade |
| **Dev email target** | (a) smtp4dev catcher container (web UI); (b) log the link to console; (c) require a real SMTP account | **(a) smtp4dev** — real SMTP path exercised end-to-end, mail viewable at :5000, nothing leaves the machine |
| **Config** | env-bound `Email__*` (`Host/Port/From/User/Password/UseSsl`) | Same vars in dev + prod; only values differ (dev → `mail:25` no-auth; prod → real relay + SSL + creds) |

## Decision

Chosen as above. `IEmailSender` (BL) + `SmtpEmailSender` (API, `System.Net.Mail`), registered in `Program.cs`; `EmailSettings` bound from the `Email` section. Dev compose adds an `rnwood/smtp4dev` service (web UI on `127.0.0.1:5000`, SMTP internal). Reset-link base URL is `App:ResetPasswordUrl`.

## Consequences

- Zero new NuGet packages; `System.Net.Mail.SmtpClient` is not recommended by MS for high-volume/modern scenarios but is fine for this transactional, low-volume, dev-first use.
- **Prod hardening (recorded, not done):** swap `SmtpEmailSender` for a MailKit or SendGrid/SES implementation behind the same `IEmailSender`; add retry/queueing; DKIM/SPF on the sending domain.
- smtp4dev is a **dev-only** container — the not-for-prod note lives alongside the db-port mapping in the compose file.

## Artifacts

- Ticket: #21 (stage 1 of 3) · Feature initiative continues the ems-completion work
