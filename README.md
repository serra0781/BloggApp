cat > README.md << 'EOF'
# BlogApp

A blog platform built with **ASP.NET Core MVC**, **Entity Framework Core**, and **ASP.NET Core Identity**.

## Features

- **Role-based access**: Admin and Author roles with different permissions
- **Article approval workflow**: authors submit posts, admins approve/reject them before they go public
- **Categories & comments**: category management, comment moderation
- **User profiles**: bio, profile photo upload, public author pages
- **Image uploads**: post cover images and profile photos, with extension/size validation

## Architecture

Controllers are kept thin — all data access and business rules live in a dedicated **service layer** (`Services/`), each exposed through an interface and registered via dependency injection. This keeps controllers focused on HTTP concerns while the actual logic (ownership checks, approval rules, etc.) stays testable and reusable.

## Tech Stack

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core + SQL Server
- ASP.NET Core Identity (authentication, roles, account lockout)

## Getting Started
