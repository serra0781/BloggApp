# BlogApp

This is a blog platform I've been building during my internship to practice ASP.NET Core MVC.

The idea is simple: writers submit articles, but an admin has to approve them before they show up publicly. Besides that there's category management, comments (which admins can moderate), and user profiles with a bio and a photo.

## Features

- Two roles: Admin and Author, each with different permissions
- Articles go through a Pending → Approved/Rejected flow
- Comment system with moderation
- Profile photos and post cover images (with size/format checks)
- Data access is split into a service layer instead of being crammed into the controllers

## Built with

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity for login/roles

## Running locally

```bash
dotnet restore
dotnet ef database update
dotnet run
