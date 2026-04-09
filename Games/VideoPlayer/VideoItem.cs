using System;

namespace VideoPlayer;

public class VideoItem
{
    public string Path { get; init; } = "";
    public string Name { get; init; } = "";
    public override string ToString() => Name;
}
