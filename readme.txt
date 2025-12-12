# 🧱 LEGO Brickster AI

> Automated data collection framework for building LEGO training datasets from BrickLink and LDraw repositories

[![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Selenium](https://img.shields.io/badge/Selenium-43B02A?style=flat&logo=selenium&logoColor=white)](https://www.selenium.dev/)
[![Status](https://img.shields.io/badge/Status-Data_Collection_Complete-green)]()

## 🎯 Overview

LEGO Brickster AI is a data collection framework designed to train an accessibility-focused AI model that can generate LEGO designs for physically disabled children. The project scrapes LEGO set data in `.mpd` and `.io` formats from BrickLink and LDraw repositories, building a comprehensive dataset for training a language model that understands natural language commands like "build a red house" or "add a roof to the structure".

### Project Vision

Traditional LEGO design software requires precise motor control, creating barriers for children with physical disabilities. By training an AI on thousands of existing LEGO designs, we aim to create a system where children can describe what they want to build in natural language, and the AI generates LDraw files that can be opened and customized in BrickLink Studio.

## 📊 Current Project Status

### ✅ Completed: Data Collection Framework
- Custom C# Selenium abstraction layer with bot detection avoidance
- Modular data collector architecture using interface-based design
- BrickLink website scraper for LEGO set data
- LDraw repository scraper for official set files
- Robust error handling and exception management
- Automated collection of `.mpd` and `.io` format files

### 🚧 Planned: AI Model Development
- LangChain integration for natural language processing
- Training pipeline for LEGO design generation
- GUI for monitoring data collection and model training
- Integration with BrickLink Studio for end-user interaction

## ✨ Key Features

**Bot Framework:**
- **Selenium Abstraction Layer**: Clean, maintainable wrapper around Selenium WebDriver
- **UndetectedChromeDriver Integration**: Avoids bot detection using [UndetectedChromeDriver](https://github.com/fysh711426/UndetectedChromeDriver)
- **Custom Exception Handling**: Specialized error types for debugging automation issues
- **Interface-Based Architecture**: Extensible design using `IGetData` interface

**Data Collection:**
- **Dual Source Collection**: Scrapes both BrickLink and LDraw websites
- **Multiple Format Support**: Collects `.mpd` and `.io` LEGO set files
- **Modular Collectors**: Easy to add new data sources by implementing `IGetData`
- **Robust & Reliable**: Production-tested data collection pipeline

## 🏗️ Architecture

```
LEGO_BRICKSTER_AI/
│
├── Project/
│   ├── Bot/
│   │   ├── Bot.cs                    # Core Selenium abstraction layer
│   │   └── BotException.cs           # Custom exception handling
│   │
│   ├── GetDataBrickLink.cs           # BrickLink website scraper
│   ├── GetDataLdraw.cs               # LDraw repository scraper
│   ├── IGetData.cs                   # Interface for data collectors
│   │
│   ├── Form1.cs                      # GUI (planned)
│   ├── Form1.Designer.cs
│   └── Program.cs                    # Entry point
│
└── Tests/
    ├── BotTests.cs                   # Unit tests for Bot class
    ├── ReportGenerator.py            # Test report generation
    └── ReportGenerator_config.yaml
```

### Architecture Flow

```
IGetData Interface
    ↓
    ├── GetDataBrickLink.cs  →  Bot.cs (Selenium Abstraction)
    │                                ↓
    └── GetDataLdraw.cs      →  UndetectedChromeDriver
                                     ↓
                              Web Scraping (BrickLink/LDraw)
                                     ↓
                              .mpd and .io files collected
```

## 🛠️ Technology Stack

**Current Implementation:**
- **C# / .NET** - Core framework
- **Selenium WebDriver** - Browser automation
- **UndetectedChromeDriver** - Bot detection avoidance
- **NUnit** - Unit testing (in Tests project)
- **Python** - Test report generation

**Planned Integration:**
- **LangChain** - Natural language processing
- **PyTorch/TensorFlow** - Model training
- **Windows Forms** - GUI for data collection monitoring

## 🚀 Usage Example

```csharp
// Initialize a data collector
IGetData brickLinkCollector = new GetDataBrickLink();

// Start collecting LEGO set data from BrickLink
await brickLinkCollector.CollectData(
    outputPath: "./collected_data",
    maxSets: 1000
);



### Bot Abstraction Layer

```csharp
// The Bot class provides a clean interface over Selenium
```

## 🗺️ Development Roadmap

### Phase 1: Data Collection ✅ COMPLETE
- [x] C# Selenium abstraction layer with UndetectedChromeDriver
- [x] Interface-based collector architecture
- [x] BrickLink scraper implementation
- [x] LDraw scraper implementation
- [x] Unit tests and error handling

### Phase 2: GUI & Monitoring 🚧 PLANNED
- [ ] Windows Forms GUI for collection control
- [ ] Real-time progress monitoring
- [ ] Collection statistics and visualization
- [ ] Configuration management UI

### Phase 3: AI Development 📋 PLANNED
- [ ] LangChain integration for NLP
- [ ] Training pipeline for design generation
- [ ] Model evaluation and testing
- [ ] Natural language to LDraw file generation

### Phase 4: Integration 📋 FUTURE
- [ ] BrickLink Studio integration
- [ ] User testing with accessibility community
- [ ] End-to-end workflow optimization
- [ ] Deployment and documentation

## 🔍 Key Design Decisions

**Why UndetectedChromeDriver?**
Web scraping at scale requires avoiding bot detection. UndetectedChromeDriver patches Selenium to bypass common detection methods, ensuring reliable long-term data collection.

**Why Interface-Based Architecture?**
The `IGetData` interface allows easy extension to new data sources. Adding a new collector (e.g., Rebrickable, Studio Gallery) only requires implementing the interface—the bot abstraction layer handles the complexity.

**Why Multiple Sources?**
- **BrickLink**: Largest community-created set database
- **LDraw**: Official, high-quality set specifications
- Combined dataset provides comprehensive training data for the AI model

## 🧪 Testing

```bash
# Run unit tests
cd Tests
dotnet test

# Generate test report
python ReportGenerator.py
```

## 🔒 Compliance & Ethics

This project follows responsible data collection practices:

- **Public Data Only**: Scrapes only publicly available LEGO set information
- **Rate Limiting**: Respectful scraping with appropriate delays
- **GDPR Alignment**: No personal data collection
- **Accessibility Mission**: All development prioritizes end-user accessibility needs

## 📧 Contact

**Daniel Sepehri**  
Software Developer & AI Specialist

- Email: Daniel.Sepehri@hotmail.com
- LinkedIn: [daniel-sepehri](https://www.linkedin.com/in/daniel-sepehri/)

---

<div align="center">

**Building a more inclusive future, one brick at a time. 🧱✨**

*The data collection framework is complete and operational. AI model development coming soon.*

</div>
