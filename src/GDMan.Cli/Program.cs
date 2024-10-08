﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

using GDMan.Cli.Options;
using GDMan.Cli.Parsing;
using GDMan.Core.Services;
using GDMan.Core.Services.Github;
using GDMan.Core.Services.FileSystem;
using GDMan.Core.Infrastructure;
using GDMan.Core.Models;
using GDMan.Cli.Version;

namespace GDMan.Cli;

class Program
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable IDE1006 // Naming Styles
    private static ServiceProvider _serviceProvider;
    private static GodotService _godot;
    private static UpdateCheckerService _updateChecker;
    private static ConsoleLogger _logger;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore IDE1006 // Naming Styles

    static async Task Main(string[] args)
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton(new ConsoleLogger(args.Contains("--verbose") || args.Contains("-vl") ? LogLevel.Trace : LogLevel.Information))
            .AddSingleton<GithubApiService>()
            .AddSingleton<GodotService>()
            .AddSingleton<UpdateCheckerService>()
            .AddSingleton<GDManRepoService>()
            .AddSingleton<Parser>()
            .AddSingleton<GDManDirectory>()
            .AddSingleton<GDManVersionsDirectory>()
            .BuildServiceProvider();

        _logger = _serviceProvider.GetRequiredService<ConsoleLogger>();
        _godot = _serviceProvider.GetRequiredService<GodotService>();
        _updateChecker = _serviceProvider.GetRequiredService<UpdateCheckerService>();

        var parser = _serviceProvider.GetRequiredService<Parser>();

        try
        {
            await _updateChecker.CheckForUpdatesIfDue();
            await HandleParseResult(parser.Parse(args));
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private static async Task HandleParseResult(ParseResult result)
    {
        if (result.HasError)
        {
            HandleError(result);
        }

        if (result.RequiresHelp)
        {
            HandleHelp(result);
        }

        if (result.RequiresVersion)
        {
            HandleVersion(result);
        }

        if (result.Options == null)
        {
            throw new NullReferenceException("Expected Options to have a value but found null");
        }

        await RunAsync(result.Options);
    }

    private static Task RunAsync(BaseOptions command) => command switch
    {
        InstallOptions i => RunInstallAsync(i),
        ListOptions i => RunListAsync(i),
        CurrentOptions i => RunCurrentAsync(i),
        UninstallOptions i => RunUninstallAsync(i),
        UpdateOptions i => RunUpdateAsync(i),
        _ => throw new InvalidOperationException()
    };

    private static async Task RunUninstallAsync(UninstallOptions command)
    {
        _logger.LogTrace("Processing uninstall command");

        var result = await _godot.UninstallAsync(
            command.Version,
            command.Platform,
            command.Architecture,
            command.Flavour,
            command.Force,
            command.Unused
        );

        HandleResult(result);

        _logger.LogTrace("Done");
    }

    private static async Task RunInstallAsync(InstallOptions command)
    {
        _logger.LogTrace("Processing install command");

        var result = await _godot.InstallAsync(
            command.Version,
            command.Latest,
            command.Platform,
            command.Architecture,
            command.Flavour
        );

        HandleResult(result);

        _logger.LogTrace("Done");
    }

    private static async Task RunListAsync(ListOptions command)
    {
        _logger.LogTrace("Processing list command");

        await _godot.ListAsync();

        _logger.LogTrace("Done");
    }

    private static async Task RunCurrentAsync(CurrentOptions command)
    {
        _logger.LogTrace("Processing current command");

        await _godot.GetCurrentVersion();

        _logger.LogTrace("Done");
    }

    private static async Task RunUpdateAsync(UpdateOptions command)
    {
        _logger.LogTrace("Processing current command");

        await _updateChecker.CheckForUpdates();

        _logger.LogTrace("Done");
    }


    [DoesNotReturn]
    private static void HandleHelp(ParseResult cliArgs)
    {
        _logger.LogInformation(cliArgs.HelpInfoString);
        Environment.Exit(0);
    }

    [DoesNotReturn]
    private static void HandleVersion(ParseResult parseResult)
    {
        _logger.LogInformation(CliVersionInfo.ToString());
        Environment.Exit(0);
    }

    [DoesNotReturn]
    private static void HandleError(ParseResult result)
    {
        _logger.LogError(string.Join(Environment.NewLine, new List<string>(result.Errors.Append(""))));
        _logger.LogInformation(result.HelpInfoString);
        Environment.Exit(1);
    }

    [DoesNotReturn]
    private static void HandleException(Exception ex)
    {
        _logger.LogFatal(ex);
        Environment.Exit(1);
    }

    private static void HandleResult<T>(Result<T> result)
    {
        if (result.Messages.Any())
        {
            var level = result.Status == ResultStatus.OK ? LogLevel.Information : LogLevel.Error;
            result.Messages.ForEach(message => _logger.Log(level, message));
        }
        if (result.Status != ResultStatus.OK)
        {
            Environment.Exit(1);
        }
    }
}