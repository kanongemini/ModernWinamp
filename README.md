# ModernWinamp

 é um player de música moderno desenvolvido em C# com .NET MAUI para Windows. O projeto combina uma interface Dark Mode inspirada no Spotify com o visual marcante do Winamp clássico, incluindo o icônico raio em neonModernWinamp.

 ## Funcionalidades

### Interface Neon

- Tema Dark Mode com fundo escuro e alto contraste.
- Raio clássico do Winamp desenhado com `Path` geométrico.
- Cores neon roxo e azul, sombras e brilho visual.
- Controles de Play, Pause, Voltar e Avançar.
- Controle de volume com botões para aumentar e diminuir o som.
- Contador do tempo da música em reprodução.

### Playlist

- Playlist inicial com músicas demonstrativas.
- Destaque visual da faixa selecionada.
- Navegação entre as faixas usando os controles do player.
- Exibição do título, artista, gênero e álbum da música.

### Importação de arquivos MP3 por Stream

- Botão `Importar MP3` para abrir o seletor de arquivos do Windows.
- Leitura do arquivo com `await result.OpenReadAsync()`.
- Reprodução direta do `Stream` usando `Plugin.Maui.Audio`.
- Leitura de metadados ID3, como título, artista e álbum, usando `TagLibSharp`.
- Fallback para informações padrão quando o arquivo não possui metadados.

## Tecnologias

- C#
- .NET 10
- .NET MAUI
- XAML
- `Plugin.Maui.Audio`
- `TagLibSharp`


  ## Imagem
  
  <img width="1058" height="607" alt="Captura de Tela (1242)" src="https://github.com/user-attachments/assets/a74b0647-7a95-4d60-a444-b682f86a00b9" />



## Requisitos

- Windows 10 versão 1809 ou superior.
- .NET 10 SDK com o workload do .NET MAUI instalado.
- Visual Studio 2022 ou Visual Studio Code com as extensões .NET e .NET MAUI.

Confira os workloads instalados com:

```powershell
dotnet workload list
```

Caso necessário, instale o workload MAUI com:

```powershell
dotnet workload install maui
```

## Como executar

Na pasta do projeto, restaure os pacotes e compile o target Windows:

```powershell
dotnet restore
dotnet build ModernWinamp.csproj --framework net10.0-windows10.0.19041.0
```

Para gerar uma versão autocontida, que não exige a instalação manual do .NET Desktop Runtime:

```powershell
dotnet publish ModernWinamp.csproj `
	--configuration Release `
	--framework net10.0-windows10.0.19041.0 `
	--runtime win-x64 `
	--self-contained true `
	--output bin\Release\win-x64\publish
```

Execute o arquivo publicado:

```powershell
Start-Process .\bin\Release\win-x64\publish\ModernWinamp.exe
```

Também é possível criar um atalho para esse executável na Área de Trabalho. Evite abrir versões antigas dentro de `bin\Debug`, pois elas podem ser framework-dependent e solicitar o .NET Desktop Runtime.

## Estrutura principal

```text
ModernWinamp/
├── MainPage.xaml          # Interface do player
├── MainPage.xaml.cs       # Controles, playlist e importação MP3
├── MauiProgram.cs         # Configuração do app e áudio
├── ModernWinamp.csproj    # Target Windows e dependências NuGet
└── Resources/             # Fontes, imagens e estilos
```

## Licença

Este projeto é destinado a fins de estudo e demonstração de uma interface de player de música com .NET MAUI.


## <img width="171" height="228" alt="Captura de Tela (1243)" src="https://github.com/user-attachments/assets/f6d982b7-a51c-4b03-bb06-7bc630d66217" />


