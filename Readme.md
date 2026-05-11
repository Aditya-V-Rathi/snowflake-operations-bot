# SnowflakeBot — Slack-Snowflake Integration

A full-stack application that enables authorized team members to perform Snowflake user management operations directly from Slack using slash commands. All operations are logged and viewable via an Angular audit dashboard.

---

## Tech Stack

- Frontend: Angular 21 (NgModule)
- Backend: C# ASP.NET Web API (.NET 8)
- Snowflake Connector: Snowflake.Data NuGet package
- Audit Database: SQLite
- Tunneling (dev): ngrok
- Containerization: Docker + Docker Compose

---

## Features

- /snowflake onboard_user username role — creates a new Snowflake user and assigns a role
- /snowflake reset_password username — resets password for a managed user
- Slack signature verification — only genuine Slack requests are processed
- Managed user authorization — only users onboarded via the system can have their password reset
- Audit log dashboard — view all command executions with status, filters, and auto-refresh

---

## Prerequisites

- Docker and Docker Compose installed
- A Slack workspace with admin access
- A Snowflake account (free trial at snowflake.com)
- ngrok (for local development only)

---

## Setup and Running

1. Clone the repository and navigate to the folder

2. Copy .env.example to .env and fill in your values:
   SLACK_SIGNING_SECRET=your-slack-signing-secret
   SNOWFLAKE_ACCOUNT=your-account-identifier
   SNOWFLAKE_USER=your-snowflake-username
   SNOWFLAKE_PASSWORD=your-snowflake-password
   SNOWFLAKE_DATABASE=SNOWFLAKE_DB
   SNOWFLAKE_SCHEMA=PUBLIC
   SNOWFLAKE_WAREHOUSE=COMPUTE_WH
   AUTHORIZED_SLACK_USER=your-slack-user-id

3. Run with Docker:
   docker-compose up --build

4. Angular dashboard will be at http://localhost:8000
   C# API will be at http://localhost:5170

5. Configure Slack slash command Request URL:
   Local dev: https://your-ngrok-url.ngrok-free.app/api/slack/command

---

## Slack Commands

/snowflake onboard_user username role
Example: /snowflake onboard_user john PUBLIC

/snowflake reset_password username
Example: /snowflake reset_password john

---

## DB Configuration

SQLite is used for audit logging. The database file auditlog.db is automatically created on first run.

AuditLogs Table:
- Id: Auto-increment primary key
- SlackUserId: Slack user ID of the requester
- SlackUsername: Slack username of the requester
- Command: Command executed (onboard_user, reset_password)
- Parameters: Full command text including arguments
- Status: success or failed
- Message: Result or error message
- Timestamp: UTC time of execution

ManagedUsers Table:
- Id: Auto-increment primary key
- Username: Snowflake username
- Role: Assigned Snowflake role
- CreatedBy: Slack username who onboarded the user
- CreatedAt: UTC time of onboarding

---

## Authorization and Security Rules

1. Slack Signature Verification: every request is verified using HMAC-SHA256 against the Slack signing secret. Requests older than 5 minutes are rejected to prevent replay attacks.

2. Authorized Slack Users: only Slack users listed in AuthorizedSlackUsers config can execute commands.

3. Managed User Authorization: reset_password can only be performed on users onboarded via this system, preventing accidental resets of system accounts like ACCOUNTADMIN.

---

## Project Structure

my-app/                         Angular Frontend
  src/app/
    components/dashboard/       Audit log dashboard
    services/audit.service.ts   API calls to backend
  Dockerfile
  nginx.conf

my-app-api/                     C# Backend
  Controllers/
    SlackController.cs          Receives Slack commands
    AuditController.cs          Serves audit logs to Angular
  Services/
    SlackService.cs             Command routing and authorization
    SnowflakeService.cs         Snowflake SQL operations
    AuditService.cs             SQLite read and write
    SlackSignatureService.cs    Request verification
  Models/
    AuditLog.cs                 Audit log model
  Data/
    DatabaseInitializer.cs      SQLite table creation
  Dockerfile

docker-compose.yml
.env                            Secrets — not committed to git
.env.example                    Template for .env
README.md

---

## Security Notes

- Never commit .env to source control
- The .env file is listed in .gitignore
- Slack signing secret is verified on every request
- Snowflake credentials are passed via environment variables, not hardcoded
