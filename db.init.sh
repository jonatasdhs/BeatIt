#!/bin/bash
# Aguarda o SQL Server iniciar completamente
sleep 30s

# Conecta ao SQL Server e cria o banco de dados se não existir
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BeatItDB') CREATE DATABASE BeatItDB;"
