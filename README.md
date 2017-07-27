# WindowTitleWatcher

[![Build status](https://ci.appveyor.com/api/projects/status/wcj98yq1x83fa7yf?svg=true)](https://ci.appveyor.com/project/meyfa/csharp-windowtitlewatcher)
[![NuGet](https://img.shields.io/nuget/v/WindowTitleWatcher.svg)](https://www.nuget.org/packages/WindowTitleWatcher)

Allows retrieving and observing window titles in C#.

## Usage

After the library has been added to your project, it is available within the
namespaces `WindowTitleWatcher` and `WindowTitleWatcher.Util`. You can then
watch foreign windows in two different ways.

### 1. Direct watching (`BasicWatcher`)

`WindowTitleWatcher.BasicWatcher` is an extension of the abstract `Watcher`
class and provides facilities for accessing a specific window's title,
visibility state, and also whether that window still exists. Events are included
that fire when any of the aforementioned properties change.

`BasicWatcher` instances can be constructed with any of the following:
- a `System.Diagnostics.Process` object (whose main window will be chosen)
- a `WindowTitleWatcher.Util.WindowInfo` object
- a window handle in the form of an `IntPtr`

The watcher can optionally keep its own process alive as long as the watched
window exists, although that is not default behavior.

Example:

```csharp
using WindowTitleWatcher;
using System.Diagnostics;
// ...

var watcher = new BasicWatcher(Process.GetProcessById(12345));
Console.WriteLine(watcher.Title); // log current window title
Console.WriteLine(watcher.IsVisible);

watcher.TitleChanged += (sender, e) =>
{
    Console.WriteLine(e.PreviousTitle + " -> " + e.NewTitle);
};
watcher.VisibilityChanged += (sender, e) =>
{
    Console.WriteLine(watcher.IsVisible ? "shown" : "hidden");
};
watcher.Disposed += (sender, e) =>
{
    Console.WriteLine("Window is disposed and cannot be watched further.");
    // The foreign process has disposed the watched window.
    // At this point, the watcher thread will exit.
};
```

### 2. Dynamic watching (`RecurrentWatcher`)

A `RecurrentWatcher` is used when the watching should not stop after the
window's disposal. It will then later reactivate when another window matching
some criteria appears.

For this, it uses a generator function supplied by you, which should return
quickly as it may be called hundreds of times each second. Its return value
should either be a `WindowInfo` instance (or `null` if none found at the
moment), or, alternatively, an `IntPtr` to the window handle (or `IntPtr.Zero`
if none found).

```csharp
using WindowTitleWatcher;
// ...

var watcher = new RecurrentWatcher(() => /* your generator function */);
```

### Window enumeration and filtered lookup

Details about the target window, such as its handle or the process's id (PID),
are often unknown. This is why the `WindowTitleWatcher.Util.Windows` class
provides static methods for enumerating all active windows as well as for
finding windows matching your criteria.

Example for finding a window for the Notepad application:

```csharp
using WindowTitleWatcher;
using WindowTitleWatcher.Util;
// ...

WindowInfo window = Windows.FindFirst(w => w.IsVisible && w.ProcessName == "notepad");
if (window != null)
{
    // The window exists, create a watcher:
    BasicWatcher watcher = new BasicWatcher(window);
    // ...
}
```

### Using window lookup with `RecurrentWatcher`

The most powerful setup is using `Windows.FindFirst` as a generator for the
`RecurrentWatcher`, as it is able to very reliably watch any window even after
it has been closed and only later reopened.

Example:

```csharp
var watcher = new RecurrentWatcher(() => Windows.FindFirst(w => w.ProcessName == "notepad"));
watcher.TitleChanged += (sender, e) => Console.WriteLine(e.NewTitle);
```
