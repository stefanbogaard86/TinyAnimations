# Tiny animations
Tiny animations to add some extra flair to any .NET application.


## ✨ Features
- `DotAnimation`: Animate using a string by appending dots (`.`, `..`, `...`, etc.) in a loop while an operation is running, providing a tiny subtle loading indication.

## 🔁 Usage

### Scoped (using `await using`):
```csharp
await using (ScopedDotAnimator.Start(text => StatusText = text, "Loading"))
{
    await LongOperationAsync();
}
StatusText = "Done!";
```

### Manual start/stop:
```csharp
var DotAnimator = DotAnimator.Start(text => StatusText = text, "Loading");
await LongOperationAsync();
DotAnimator.Stop();
```

## 📦 Installation
dotnet add package TinyAnimations --version 1.0.1

