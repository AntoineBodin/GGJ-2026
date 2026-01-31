# Alien Detection Instruction System

## Overview

This system generates rules to detect aliens hiding as humans in dating profiles. Each round, random detection rules are generated, and profiles are created that either match (aliens) or don't match (humans) these rules.

---

## System Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         ROUND START                              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  1. RoundGenerationConfig (ScriptableObject)                    │
│     - Defines which attributes can be used (Age, Height, etc.)  │
│     - Defines human vs alien ranges for each attribute          │
│     - Configures logic type (AND/OR/Tree)                       │
│     - Sets number of conditions per round                       │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  2. RoundRuleGenerator                                          │
│     - Picks random attributes based on weights                  │
│     - Creates instructions with appropriate comparators         │
│     - Combines into single rule (AND/OR/Tree)                   │
│     - Output: GeneratedRule                                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  3. GeneratedRule                                               │
│     - CombinedRule: IInstruction (the detection rule)           │
│     - Instructions: List of individual rules with metadata      │
│     - LogicType: How rules are combined                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  4. ProfileGenerator                                            │
│     - GenerateAlienProfile(): Creates profile matching rules    │
│     - GenerateHumanProfile(): Creates profile NOT matching      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  5. Gameplay                                                    │
│     - Player sees profile                                       │
│     - Player uses rules to decide: alien or human?              │
│     - Check: rule.CombinedRule.IsValid(profile)                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## File Structure

```
Assets/_Scripts/
├── Model/
│   ├── Profile.cs                    # Profile data model
│   ├── Gender.cs                     # Gender enum
│   ├── ProfilePictureElements.cs     # Visual elements
│   │
│   └── Instructions/
│       ├── IInstruction.cs           # Core interface
│       ├── InstructionBase.cs        # Generic base class
│       │
│       ├── Concrete Instructions:
│       │   ├── AgeInstruction.cs
│       │   ├── HeightInstruction.cs
│       │   ├── InterestCountInstruction.cs
│       │   ├── GenderInstruction.cs
│       │   ├── EyeColorInstruction.cs
│       │   ├── HairColorInstruction.cs
│       │   └── FaceColorInstruction.cs
│       │
│       ├── Composite Instructions:
│       │   ├── CompositeInstruction.cs
│       │   ├── AndInstruction.cs
│       │   └── OrInstruction.cs
│       │
│       ├── Enums:
│       │   ├── AttributeType.cs
│       │   └── LogicType.cs
│       │
│       └── Comparators/
│           ├── IComparator.cs
│           ├── EqualsComparator.cs
│           ├── GreaterComparator.cs
│           ├── GreaterOrEqualsComparator.cs
│           ├── LowerComparator.cs
│           ├── LowerOrEqualsComparator.cs
│           └── ColorEqualsComparator.cs
│
├── ScriptableObjects/
│   ├── RoundGenerationConfig.cs      # Main config
│   ├── NumericAttributeConfig.cs     # Numeric attribute settings
│   └── ColorAttributeConfig.cs       # Color attribute settings
│
├── Managers/
│   ├── GameManager.cs                # Manages game state & rounds
│   ├── RoundRuleGenerator.cs         # Generates rules
│   └── ProfileGenerator.cs           # Generates profiles
│
└── Testing/
    └── InstructionSystemTester.cs    # Test script
```

---

## How to Use

### 1. Setup in Unity

#### Create Config Asset
1. Right-click in Project window
2. Create → Scriptable Objects → **RoundGenerationConfig**
3. Configure in Inspector

#### Assign to GameManager
1. Select GameManager in scene
2. Drag config asset to `Config` field

### 2. Configure Rules

In the RoundGenerationConfig Inspector:

```
Logic Settings:
  Logic Type: [And / Or / Tree]
  Min Conditions: 1
  Max Conditions: 3

Numeric Attributes:
  [0] Age
      Enabled: ✓
      Weight: 1.0
      Human Min: 18    Human Max: 60
      Alien Min: 28    Alien Max: 32

  [1] Height
      Enabled: ✓
      Weight: 1.0
      Human Min: 1.5   Human Max: 1.9
      Alien Min: 1.9   Alien Max: 2.2

  [2] InterestCount
      Enabled: ✓
      Weight: 1.0
      Human Min: 3     Human Max: 8
      Alien Min: 1     Alien Max: 2

Color Attributes:
  [0] EyeColor
      Enabled: ✗
      ...
```

### 3. In Code

#### Start New Round
```csharp
// Automatically happens in GameManager.Start()
// Or manually:
GameManager.Instance.StartNewRound();
```

#### Get Current Rules
```csharp
GeneratedRule rule = GameManager.Instance.CurrentRule;

// Display rules to player
foreach (var instruction in rule.Instructions) {
    Debug.Log(instruction.Description);  // "Age >= 28", "Height >= 1.90"
}
```

#### Generate Profiles
```csharp
// Generate an alien (matches rules)
Profile alien = ProfileGenerator.Instance.GenerateAlienProfile();

// Generate a human (doesn't match rules)
Profile human = ProfileGenerator.Instance.GenerateHumanProfile();
```

#### Check if Profile is Alien
```csharp
Profile profile = GetCurrentProfile();
bool isAlien = GameManager.Instance.CurrentRule.CombinedRule.IsValid(profile);

if (isAlien) {
    // Profile matches alien detection rules
} else {
    // Profile is human
}
```

#### Listen to Round Changes
```csharp
void Start() {
    GameManager.OnNewRound += OnNewRound;
}

void OnNewRound(GeneratedRule rule) {
    // Update UI with new rules
    UpdateRulesDisplay(rule);
}
```

---

## How to Extend

### Adding a New Attribute

#### Step 1: Add to AttributeType enum
```csharp
// In AttributeType.cs
public enum AttributeType {
    Age,
    Height,
    InterestCount,
    Gender,
    EyeColor,
    HairColor,
    FaceColor,
    JobTitle        // NEW
}
```

#### Step 2: Add field to Profile (if needed)
```csharp
// In Profile.cs
public class Profile {
    public string Name;
    public int Age;
    public float Height;
    public string JobTitle;  // NEW
    // ...
}
```

#### Step 3: Create Instruction class
```csharp
// New file: JobTitleInstruction.cs
using Assets._Scripts.Model.Instructions.Comparators;

namespace Assets._Scripts.Model.Instructions {
    public class JobTitleInstruction : InstructionBase<string> {
        public JobTitleInstruction(string expectedValue, IComparator<string> comparator)
            : base(expectedValue, comparator) { }

        public override string GetValue(Profile profile) {
            return profile.JobTitle;
        }
    }
}
```

#### Step 4: Update RoundRuleGenerator
Add the instruction creation in the appropriate method:
```csharp
// In RoundRuleGenerator.cs, add to CreateIntInstruction or create new method
private IInstruction CreateStringInstruction(AttributeType type, string value, IComparator<string> comparator) {
    switch (type) {
        case AttributeType.JobTitle:
            return new JobTitleInstruction(value, comparator);
        default:
            return null;
    }
}
```

#### Step 5: Update ProfileGenerator
Add handling in ApplyAlienValue and ApplyHumanValue methods.

#### Step 6: Add Config
Add a new config type or use existing NumericAttributeConfig if appropriate.

---

## Logic Types Explained

### AND Logic
All conditions must be true for detection.

```
Rules: Age >= 28 AND Height >= 1.90 AND InterestCount <= 2

Profile: Age=30, Height=2.0, Interests=1
Result: TRUE (all match)

Profile: Age=30, Height=1.7, Interests=1
Result: FALSE (height fails)
```

### OR Logic
Any condition being true triggers detection.

```
Rules: Age >= 28 OR Height >= 1.90 OR InterestCount <= 2

Profile: Age=25, Height=1.7, Interests=5
Result: FALSE (none match)

Profile: Age=25, Height=2.0, Interests=5
Result: TRUE (height matches)
```

### Tree Logic
Depth = AND, Width = OR

```
        [Root]
        /    \
    [A]      [B]     <- A OR B
    /
  [C]                <- A AND C

Result: (Root AND A AND C) OR (Root AND B)
```

---

## Comparators

| Comparator | Symbol | Example |
|------------|--------|---------|
| EqualsComparator | == | Age == 30 |
| GreaterComparator | > | Age > 28 |
| GreaterOrEqualsComparator | >= | Age >= 28 |
| LowerComparator | < | Age < 60 |
| LowerOrEqualsComparator | <= | Age <= 32 |
| ColorEqualsComparator | == | EyeColor == Green |

---

## Testing

### Using InstructionSystemTester

1. Create empty GameObject
2. Add `InstructionSystemTester` component
3. Assign your `RoundGenerationConfig`
4. Enter Play Mode

**Controls:**
- **G** - Generate new rules
- **P** - Generate test profiles
- **T** - Run full test (AND/OR/Tree)

### Expected Output
```
===== Testing LogicType: And =====
Rules:
  - Height >= 1.90
  - Age >= 28
Results: Aliens detected 10/10, Humans passed 10/10
```

---

## Best Practices

### Config Design
1. **Non-overlapping ranges work best** - Clear separation between human and alien ranges
2. **Overlapping ranges add difficulty** - Some uncertainty for players
3. **Adjust weights** - Control which attributes appear more often

### Range Examples

**Easy (no overlap):**
```
Human Age: 18-50
Alien Age: 55-70
```

**Medium (slight overlap):**
```
Human Age: 18-60
Alien Age: 55-70
```

**Hard (significant overlap):**
```
Human Age: 18-60
Alien Age: 28-32
```

### Performance
- Rules are generated once per round
- Profile validation is O(n) where n = number of conditions
- Suitable for real-time gameplay

---

## Troubleshooting

### "Aliens not detected"
- Check if profile values are within alien range
- Verify all conditions are met (for AND logic)
- Check config ranges in Inspector

### "Humans detected as aliens"
- Human ranges may overlap with alien ranges
- Check if profile generation is using human ranges correctly

### "Duplicate attribute types"
- Each AttributeType should appear only once in config
- Check Inspector for duplicate entries

### "Rule shows wrong ranges"
- Verify AttributeType dropdown is set correctly for each config entry
- InterestCount config shouldn't have AttributeType set to Height, etc.

---

## API Reference

### IInstruction
```csharp
interface IInstruction {
    bool IsValid(Profile profile);
}
```

### GeneratedRule
```csharp
class GeneratedRule {
    IInstruction CombinedRule;           // Combined rule to check profiles
    List<GeneratedInstruction> Instructions;  // Individual rules with metadata
    LogicType LogicType;                 // How rules are combined
}
```

### GeneratedInstruction
```csharp
class GeneratedInstruction {
    IInstruction Instruction;            // The instruction
    AttributeType AttributeType;         // Which attribute
    string Description;                  // Human-readable: "Age >= 28"
    float AlienValue;                    // Value used in rule
    NumericAttributeConfig NumericConfig;  // Original config
    ColorAttributeConfig ColorConfig;    // For color attributes
}
```

### Key Methods
```csharp
// GameManager
GameManager.Instance.StartNewRound();
GameManager.Instance.CurrentRule;
GameManager.OnNewRound += (rule) => { };

// ProfileGenerator
ProfileGenerator.Instance.GenerateAlienProfile();
ProfileGenerator.Instance.GenerateHumanProfile();

// Rule checking
bool isAlien = rule.CombinedRule.IsValid(profile);
```
