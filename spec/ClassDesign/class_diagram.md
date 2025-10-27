```mermaid
classDiagram
    class MainWindow {
        <<View>>
    }
    class SettingsWindow {
        <<View>>
    }

    class MainWindowViewModel {
        <<ViewModel>>
        +MarkdownContent: string
        +CurrentMode: DisplayMode
        +SelectedCss: string
        +FolderTreePosition: FolderTreePosition
        +FileTree: IEnumerable<FileTreeNode>
        +OpenFileCommand: ICommand
        +OpenFolderCommand: ICommand
        +SaveFileCommand: ICommand
        +ExportCommand: ICommand
        +OpenSettingsCommand: ICommand
    }
    class SettingsViewModel {
        <<ViewModel>>
        +FolderTreePosition: FolderTreePosition
        +SaveSettingsCommand: ICommand
        +CancelSettingsCommand: ICommand
    }

    class FileTreeNode {
        +Name: string
        +FilePath: string
        +IsDirectory: bool
        +Children: IEnumerable<FileTreeNode>
    }
    class MarkdownDocument {
        +Content: string
        +FilePath: string
    }
    class ApplicationSettings {
        +FolderTreePosition: FolderTreePosition
        +SelectedCss: string
        +LastOpenedFolder: string
    }

    class IFileService {
        <<Interface>>
        +Open(filePath): MarkdownDocument
        +Save(document): void
        +OpenFolder(folderPath): IEnumerable<FileTreeNode>
    }
    class FileService {
        +Open(filePath): MarkdownDocument
        +Save(document): void
        +OpenFolder(folderPath): IEnumerable<FileTreeNode>
    }

    class IExportService {
        <<Interface>>
        +Export(document, format): void
    }
    class ExportService {
        +Export(document, format): void
    }

    class ISettingsService {
        <<Interface>>
        +LoadSettings(): ApplicationSettings
        +SaveSettings(settings): void
    }
    class SettingsService {
        +LoadSettings(): ApplicationSettings
        +SaveSettings(settings): void
    }

    MainWindow --> MainWindowViewModel
    SettingsWindow --> SettingsViewModel

    MainWindowViewModel --> MarkdownDocument
    MainWindowViewModel --> IFileService
    MainWindowViewModel --> IExportService
    MainWindowViewModel --> ISettingsService
    MainWindowViewModel --> ApplicationSettings
    MainWindowViewModel --> FileTreeNode

    SettingsViewModel --> ISettingsService
    SettingsViewModel --> ApplicationSettings

    FileService ..|> IFileService
    ExportService ..|> IExportService
    SettingsService ..|> ISettingsService

    ApplicationSettings "1" -- "1" ISettingsService : uses
    MarkdownDocument "1" -- "1" IFileService : uses
```
