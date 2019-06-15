﻿namespace IOTLinkAgent.Agent.Commands
{
    public interface ICommand
    {
        string GetCommandLine();
        int ExecuteCommand(string[] args);
    }
}
