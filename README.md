# Alchemist

A Unity-based 2D RPG adventure game focused on alchemy, exploration, and character progression. Master the art of potion brewing while exploring procedurally generated worlds, battling enemies, and uncovering the secrets of alchemy.

## 📸 Screenshots

*Screenshots and gameplay videos will be added as development progresses*

## 🎮 Game Overview

Alchemist is an immersive RPG where players take on the role of an aspiring alchemist. Venture through different environments, collect rare materials, craft powerful potions, and face challenging enemies in your quest to become a master alchemist.

### Key Features

- **🧪 Advanced Alchemy System**: Craft potions using complex recipes with various reagents and brewing times
- **🗺️ Procedural World Generation**: Explore dynamically generated maps including towns, resource areas, enemy territories, and boss rooms
- **⚔️ Combat & Character Progression**: Engage in tactical combat with a variety of weapons and abilities while developing your character's skills and equipment
- **🎒 Rich Inventory Management**: Comprehensive inventory system with equipment, consumables, materials, and tools
- **🏪 Trading & Economics**: Interact with merchants and manage resources across different locations
- **📚 Recipe Discovery**: Unlock new alchemy recipes and expand your potion-making capabilities
- **💬 NPC Interactions & Conversations**: Engage with NPCs through dynamic conversation systems
- **📋 Quest System**: Track and complete various quests and objectives
- **🎵 Immersive Audio**: Dynamic audio system that enhances the gaming experience

## 🎯 Core Gameplay Mechanics

### Alchemy System
- **Recipe Crafting**: Combine reagents according to discovered recipes
- **Brewing Process**: Real-time brewing with crafting timers
- **Potion Categories**: Different types of potions with various effects
- **Byproducts**: Manage waste and secondary outputs from brewing
- **Quality Control**: Craft potions with varying effectiveness

### Exploration
- **Multiple Biomes**: Town areas, resource-rich zones, enemy territories, and boss encounters
- **Dynamic Maps**: Procedurally generated layouts using advanced algorithms
- **Hidden Secrets**: Discover rare materials and secret locations
- **Environmental Interactions**: Gather resources and interact with the world

### Character Development
- **Skill Progression**: Advance through different player ranks and abilities
- **Equipment System**: Equip weapons, armor, and tools to enhance capabilities
- **Attribute Management**: Develop various character attributes and stats
- **Ability Unlocks**: Gain access to new abilities and techniques

### Social Systems
- **NPC Interactions**: Engage in meaningful conversations with non-player characters
- **Quest Management**: Accept, track, and complete various objectives and storylines
- **Dialogue System**: Dynamic conversation system with multiple response options
- **Relationship Building**: Develop relationships with NPCs through interactions

## 🛠️ Technical Requirements

### System Requirements
- **Unity Version**: Unity 6000.0.27f1 or later
- **Platform**: Windows, macOS, Linux
- **Graphics**: DirectX 11 compatible graphics card
- **Memory**: 4 GB RAM minimum, 8 GB recommended
- **Storage**: 2 GB available space

### Development Dependencies
- Unity Input System
- 2D Animation Package
- AI Navigation
- Timeline
- Visual Scripting
- Super Tiled2Unity (for map integration)
- A* Pathfinding Project

## 🚀 Getting Started

### For Players
1. Download the latest release from the releases page
2. Extract the game files to your desired location
3. Run the executable file
4. Start your alchemical journey!

### For Developers

#### Prerequisites
- Unity 6000.0.27f1 installed
- Git for version control
- Code editor (Visual Studio, Rider, or VS Code recommended)

#### Setup Instructions
1. Clone the repository:
   ```bash
   git clone https://github.com/Daphatus7/Alchemist.git
   ```

2. Open Unity Hub and add the project:
   - Click "Add" in Unity Hub
   - Navigate to the cloned repository folder
   - Select the project folder

3. Open the project in Unity:
   - Wait for Unity to import all assets and packages
   - The project will automatically resolve dependencies

4. Configure the project:
   - Ensure all packages are properly installed
   - Verify that the A* Pathfinding Project is configured
   - Check that Super Tiled2Unity is properly set up

#### Building the Game
1. Open Build Settings (File > Build Settings)
2. Select your target platform
3. Configure build settings as needed
4. Click "Build" or "Build and Run"

## 📁 Project Structure

```
Assets/
├── _Script/           # Core game scripts organized by system
│   ├── Alchemy/       # Alchemy and potion crafting system
│   ├── Character/     # Player character and progression
│   ├── Enemy/         # Enemy AI and behavior
│   ├── Inventory/     # Inventory and item management
│   ├── Items/         # Item definitions and types
│   ├── Managers/      # Core game management systems
│   ├── Map/           # Map generation and management
│   └── ...
├── Level/             # Game scenes and levels
├── Resources/         # Runtime-loaded assets
├── _Prefabs/          # Reusable game objects
├── Tiles/             # Tilemap assets
└── ...

ProjectSettings/       # Unity project configuration
Packages/             # Package dependencies
```

## 🧑‍💻 Development Guidelines

### Code Architecture
- **Singleton Pattern**: Used for core managers (GameManager, UIManager, etc.)
- **Service Locator**: For dependency injection and service management
- **Component-Based Design**: Modular components for game functionality
- **State Machines**: For character and game state management

### Coding Standards
- Use meaningful variable and method names
- Follow Unity's C# coding conventions
- Document complex systems with XML comments
- Organize scripts in appropriate namespace structures

### Git Workflow
- Create feature branches for new development
- Write descriptive commit messages
- Test changes thoroughly before merging
- Keep the main branch stable and buildable

## 🤝 Contributing

We welcome contributions to the Alchemist project! Here's how you can help:

### Getting Involved
1. Fork the repository
2. Create a feature branch for your changes
3. Make your improvements or fixes
4. Test your changes thoroughly
5. Submit a pull request with a clear description

### Areas for Contribution
- **Bug Fixes**: Help identify and resolve issues
- **Feature Development**: Implement new gameplay mechanics
- **UI/UX Improvements**: Enhance the user interface and experience
- **Performance Optimization**: Improve game performance and efficiency
- **Documentation**: Improve code documentation and guides
- **Art Assets**: Contribute sprites, animations, or audio

### Reporting Issues
- Use the GitHub Issues tab to report bugs
- Provide clear reproduction steps
- Include system information and Unity version
- Add screenshots or logs when relevant

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Credits

### Development Team
- **Peiyu Wang** (@Daphatus) - Lead Developer and Project Creator

### Third-Party Assets
- **A* Pathfinding Project** - AI navigation and pathfinding
- **Super Tiled2Unity** - Tilemap integration
- **Unity Technologies** - Game engine and various packages

### Special Thanks
- Unity community for tutorials and resources
- Open source contributors for various tools and libraries
- Beta testers and community feedback

## 📞 Contact

- **GitHub**: [@Daphatus7](https://github.com/Daphatus7)
- **Project Repository**: [Alchemist](https://github.com/Daphatus7/Alchemist)

## 🔄 Version History

### Current Development
- Active development of core alchemy systems
- Implementation of procedural map generation
- Character progression and combat mechanics
- UI/UX refinements and optimizations

## 🗺️ Development Roadmap

### Core Systems (In Progress)
- ✅ Basic alchemy and potion crafting
- ✅ Character movement and basic combat
- ✅ Inventory and equipment systems
- ✅ NPC interaction and conversation system
- 🚧 Quest system implementation
- 🚧 Advanced map generation
- 🚧 Enemy AI and abilities

### Planned Features
- 📋 Complete quest and storyline system
- 🏆 Achievement and progression rewards
- 🌍 Multiple biomes and environments
- 🎨 Enhanced visual effects and animations
- 🔊 Complete audio design and music
- 📱 UI/UX polish and accessibility features

### Future Considerations
- 🌐 Multiplayer support
- 📦 Mod support and community content
- 🏪 Steam Workshop integration
- 📱 Mobile platform support

---

*Embark on your alchemical journey and discover the mysteries that await in the world of Alchemist!*