# Changelog

All notable changes to NaticordVB are written here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
There are no tagged releases yet, so everything lives under *Unreleased*.

NaticordVB is a fork of [Naticord/naticord](https://github.com/Naticord/naticord). The starting point was upstream's C# code on
.NET Framework 4.8 (commit `9de582b`): login by token or QR code, a setup wizard, and a client window with placeholder content.

## [Unreleased]

### Added
- **Direct messages and servers in the sidebar.**
  - A *Messages* tab with your DMs and group DMs, newest activity first, with avatars and the time of the last message.
    The list refreshes by itself every 30 seconds.
  - A *Servers* tab: pick a server to see its text and announcement channels grouped by category, and go back with the top row.
  - If a list can't be loaded the sidebar says why and lets you click to try again.
- **Reading a conversation.**
  - The latest 50 messages, a "Load earlier messages" row that keeps your place, and new messages every 4 seconds while the app is in front.
  - Round avatars, names and times; messages from the same person within 7 minutes are grouped; days are separated.
  - Replies show the message they answer. Attachments, link previews and stickers appear as lines of text, and web links open in your browser.
  - System messages ("X joined the server.", "X pinned a message...") and an "(edited)" mark.
  - `<@123>`, `<@&123>`, `<#123>`, custom emoji, `<t:...>` times and slash-command tags are shown as readable text.
  - Click a message to pick it; `Ctrl+C` or right-click, *Copy text*, copies exactly what the author wrote. `Esc` lets go.
- **Sending.** A box at the bottom of the chat: `Enter` sends, `Shift+Enter` starts a new line, it grows up to 6 lines, and 2000
  characters are checked before sending. Right-click a message, *Reply*, to answer a specific message ("Replying to..." bar with *Cancel*
  or `Esc`). Every chat keeps its own unsent draft, so text never goes to the wrong chat. If sending fails the text stays, with a short
  explanation: no permission, slow down for N seconds, can't reach Discord, or "unclear whether it was sent" after a timeout.
  A message is never sent twice automatically.
- **Unread marks.** Chats with something newer than what you last saw here get a bold title and a dot. The state is kept on this PC only.
- **Status.** *Account > Your status*: online, idle, do not disturb, invisible, with a check mark that follows what Discord reports.
  *Set custom status...* takes text (up to 128 characters), one Unicode emoji and "clear after" (30 minutes, 1 hour, 4 hours, today or never),
  is prefilled with the current one and has *Remove status*.
- **Text in every script.** Sundanese (and anything else only the "Sans Serif Collection" font covers) showed up as empty boxes. Such
  characters are now drawn with a fallback font in message text, names, the sidebar, the chat title and the glass label in the toolbar.
- Long words and links without spaces are broken across lines instead of running off the edge.
- Networking foundation: `API.RequestAsync` returns an `ApiResult` (status, kind of error, Discord's message and code, `Retry-After`)
  and never throws for network or HTTP problems. 15 second timeout, one retry for a rate-limited read (never for a write), URL-encoded
  query strings. Typed models, `DiscordService`, CDN address helpers and an avatar cache that downloads 4 pictures at a time.
- A **Roadmap** in the README (what is done, what is next, what is undecided) and this changelog.

### Changed
- **The whole codebase is rewritten in VB.NET (`Option Strict On`) and moved from .NET Framework 4.8 to .NET 10.** The project is now
  SDK-style: `NaticordVB/NaticordVB.vbproj`, with a `NaticordVB.slnx` at the root. Build with
  `dotnet build NaticordVB/NaticordVB.vbproj -c Release`.
- `User Interface`, `ExtResources` and `Libraries` are copied to the output folder by the project file instead of an `xcopy` build step.
  The native `dui_bridge.dll` still has to be built separately and placed in `NaticordVB/Libraries/x64/` (see the README).
- QR codes now need the `ZXing.Net.Bindings.Windows.Compatibility` package.
- Visible text says **NaticordVB** instead of Naticord (window titles, dialogs, menus, DirectUI pages, the file properties).
- The sidebar's tabs and list are WinForms controls. DirectUI can't scroll a list and can't tell which row was clicked, so it only keeps
  the search box there. Login, setup wizard and the "Hold on!" page are still DirectUI.
- Logging in no longer saves a token blindly when Discord can't be reached: it asks first.
- The login banner follows window scaling.
- Login and the setup wizard are hidden, not closed, when the next window opens. Closing a window that hosts DirectUI ends the whole app.
  A small `NaticordContext` ends the app when the last visible window closes.
- The README's supported systems are corrected: .NET 10 does not run on Windows 7, 8.1, Vista or XP.

### Fixed
- The app crashed after about 100 seconds when it could not reach Discord. It now shows "(offline)" and keeps the saved name.
- The process kept running after the client window was closed.
- Windows version detection used a structure of the wrong size.
- A broken text in the error returned by the API layer, a wrong `Dispose` in the appearance page, and a "collection was modified" risk
  when swapping pages.

### Removed
- Dead code: an empty avatar cache, an unused older copy of the setup wizard forms, the DirectUI list item and its template.
- Packages that .NET 10 already includes (`System.Text.Json`, `System.Net.WebSockets`, `System.ValueTuple` and friends).

[Unreleased]: https://github.com/aldirafa/NaticordVB/commits/main
