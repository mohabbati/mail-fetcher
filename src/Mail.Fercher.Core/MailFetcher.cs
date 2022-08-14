﻿namespace Mail.Fercher.Core;

public class MailFetcher<TFetcher>
    where TFetcher : IFetcher
{
    private readonly TFetcher _fetcher;

    private readonly FetcherConfiguration _fetcherConfiguration;

    public MailFetcher(TFetcher fetcher) : this(fetcher, new FetcherConfiguration())
    {
    }

    public MailFetcher(TFetcher fetcher, FetcherConfiguration fetcherConfiguration)
    {
        _fetcher = fetcher;
        _fetcherConfiguration = fetcherConfiguration;
    }

    public async Task<List<MailMessage>> InvokeAsync(MailServerConnection mailServerConnection, CancellationToken cancellationToken)
    {
        await OnFetching(_fetcher, cancellationToken);

        var result = new List<MailMessage>();

        try
        {
            if (_fetcherConfiguration.ExecutionType == FetcherConfiguration.ParallelismStatus.None)
            {
                result = await _fetcher.FetchAsync(mailServerConnection, cancellationToken);
            }
            else if (_fetcherConfiguration.ExecutionType is FetcherConfiguration.ParallelismStatus.ConditionalParallel or FetcherConfiguration.ParallelismStatus.ForceParallel)
            {
                result = await _fetcher.FetchParallelAsync(_fetcherConfiguration, mailServerConnection, cancellationToken);
            }
        }
        catch (Exception)
        {
            await OnFetchFailed(_fetcher, cancellationToken);

            throw;
        }

        await OnFetched(_fetcher, cancellationToken);

        return result;
    }

    /// <summary>
    /// Gets called right before Fetch
    /// </summary>
    protected virtual Task OnFetching(TFetcher fetcher, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets called right after Fetch
    /// </summary>
    protected virtual Task OnFetched(TFetcher fetcher, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets called right after Fetch failed
    /// </summary>
    protected virtual Task OnFetchFailed(TFetcher fetcher, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}