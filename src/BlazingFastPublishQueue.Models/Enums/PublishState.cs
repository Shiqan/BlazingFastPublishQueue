namespace BlazingFastPublishQueue.Models
{
    public enum PublishState
    {
        None,
        WaitingForPublish,
        Rendering,
        WaitingForDeployment,
        Deploying,
        Success,
        Warning,
        Failed
    }
}
