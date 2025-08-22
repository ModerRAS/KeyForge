# KeyForge 架构图和组件关系图

## 1. 系统整体架构图

### 1.1 Clean Architecture 分层架构

```mermaid
graph TB
    subgraph "Presentation Layer (表现层)"
        UI[WPF Desktop UI]
        API[Web API]
        Mobile[Mobile App]
    end
    
    subgraph "Application Layer (应用层)"
        UC[Use Cases]
        CH[Command Handlers]
        AE[Application Events]
    end
    
    subgraph "Domain Layer (领域层)"
        DM[Domain Models]
        DS[Domain Services]
        DE[Domain Events]
    end
    
    subgraph "Infrastructure Layer (基础设施层)"
        DP[Data Persistence]
        ES[External Services]
        MQ[Message Queue]
        IO[Input/Output]
    end
    
    UI --> UC
    API --> CH
    Mobile --> UC
    
    UC --> DM
    CH --> DS
    AE --> DE
    
    DM --> DP
    DS --> ES
    DE --> MQ
    IO --> CH
    
    style DM fill:#f9f,stroke:#333,stroke-width:2px
    style DS fill:#f9f,stroke:#333,stroke-width:2px
    style DE fill:#f9f,stroke:#333,stroke-width:2px
```

### 1.2 DDD 边界上下文架构

```mermaid
graph TB
    subgraph "KeyForge System"
        subgraph "Automation Context (自动化执行上下文)"
            A1[Script Management]
            A2[Action Execution]
            A3[Input Recording]
        end
        
        subgraph "Vision Context (图像识别上下文)"
            V1[Image Recognition]
            V2[Screen Capture]
            V3[Template Management]
        end
        
        subgraph "Decision Context (决策引擎上下文)"
            D1[Rule Engine]
            D2[State Machine]
            D3[Expression Evaluator]
        end
        
        subgraph "Management Context (系统管理上下文)"
            M1[Configuration]
            M2[Logging]
            M3[Monitoring]
        end
        
        subgraph "Interaction Context (用户交互上下文)"
            I1[User Interface]
            I2[View Management]
            I3[Session Management]
        end
    end
    
    A1 --> V1
    V1 --> D1
    D1 --> A2
    A2 --> M1
    M1 --> I1
    
    style A1 fill:#e1f5fe,stroke:#0277bd,stroke-width:2px
    style V1 fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style D1 fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
    style M1 fill:#fce4ec,stroke:#c2185b,stroke-width:2px
    style I1 fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
```

## 2. Sense-Judge-Act 闭环架构

### 2.1 闭环架构图

```mermaid
graph LR
    subgraph "Sense Layer (感知层)"
        S1[Screen Capture]
        S2[Image Recognition]
        S3[Input Monitoring]
        S4[Environment Detection]
    end
    
    subgraph "Judge Layer (决策层)"
        J1[Rule Engine]
        J2[State Machine]
        J3[Condition Evaluator]
        J4[Decision Logic]
    end
    
    subgraph "Act Layer (执行层)"
        A1[Keyboard Input]
        A2[Mouse Input]
        A3[Script Execution]
        A4[Action Sequence]
    end
    
    S1 --> J1
    S2 --> J1
    S3 --> J2
    S4 --> J3
    
    J1 --> A1
    J2 --> A2
    J3 --> A3
    J4 --> A4
    
    A1 -.-> S1
    A2 -.-> S2
    A3 -.-> S3
    A4 -.-> S4
    
    style S1 fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style J1 fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style A1 fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
```

### 2.2 闭环数据流

```mermaid
sequenceDiagram
    participant S as Sense Layer
    participant J as Judge Layer
    participant A as Act Layer
    participant E as Environment
    
    loop Sense-Judge-Act Cycle
        S->>E: Capture Screen State
        E-->>S: Screen Data
        S->>J: Send Perception Data
        J->>J: Evaluate Rules/Conditions
        J->>A: Send Decision Commands
        A->>E: Execute Actions
        E-->>S: Updated Environment
    end
```

## 3. 组件关系和依赖图

### 3.1 核心组件依赖关系

```mermaid
graph TB
    subgraph "Domain Layer"
        DM_Script[Script Entity]
        DM_Template[ImageTemplate Entity]
        DM_Rule[DecisionRule Entity]
        DM_State[StateMachine Entity]
        
        DS_Recognition[IImageRecognitionService]
        DS_Execution[IScriptExecutionService]
        DS_Decision[IDecisionEngineService]
        
        DE_ScriptCreated[ScriptCreatedEvent]
        DE_ImageRecognized[ImageRecognizedEvent]
        DE_DecisionMade[DecisionMadeEvent]
    end
    
    subgraph "Application Layer"
        UC_Record[RecordScriptUseCase]
        UC_Execute[ExecuteScriptUseCase]
        UC_Recognize[ImageRecognitionUseCase]
        UC_Decide[DecisionMakingUseCase]
        
        CH_Script[ScriptCommandHandler]
        CH_Image[ImageCommandHandler]
        CH_Decision[DecisionCommandHandler]
    end
    
    subgraph "Infrastructure Layer"
        REPO_Script[ScriptRepository]
        REPO_Template[ImageTemplateRepository]
        REPO_Rule[DecisionRuleRepository]
        
        DB[(SQLite Database)]
        MQ[(RabbitMQ)]
        CACHE[(Redis)]
        
        EXT_OpenCV[OpenCV Service]
        EXT_Windows[Windows API]
    end
    
    UC_Record --> DM_Script
    UC_Execute --> DM_Script
    UC_Recognize --> DM_Template
    UC_Decide --> DM_Rule
    
    CH_Script --> UC_Record
    CH_Image --> UC_Recognize
    CH_Decision --> UC_Decide
    
    DM_Script --> REPO_Script
    DM_Template --> REPO_Template
    DM_Rule --> REPO_Rule
    
    REPO_Script --> DB
    REPO_Template --> DB
    REPO_Rule --> DB
    
    DS_Recognition --> EXT_OpenCV
    DS_Execution --> EXT_Windows
    
    DE_ScriptCreated --> MQ
    DE_ImageRecognized --> MQ
    DE_DecisionMade --> MQ
    
    UC_Execute --> CACHE
    UC_Recognize --> CACHE
    
    style DM_Script fill:#f9f,stroke:#333,stroke-width:2px
    style DS_Recognition fill:#f9f,stroke:#333,stroke-width:2px
    style DE_ScriptCreated fill:#f9f,stroke:#333,stroke-width:2px
```

### 3.2 消息流和事件传播

```mermaid
graph TB
    subgraph "Event Sources"
        ES_Script[Script Service]
        ES_Image[Image Service]
        ES_Decision[Decision Service]
        ES_System[System Service]
    end
    
    subgraph "Event Bus"
        EB[Event Bus]
        EH1[Script Handler]
        EH2[Image Handler]
        EH3[Decision Handler]
        EH4[System Handler]
    end
    
    subgraph "Event Consumers"
        EC_Logging[Logging Service]
        EC_Monitoring[Monitoring Service]
        EC_Cache[Cache Service]
        EC_Notification[Notification Service]
    end
    
    ES_Script --> EB
    ES_Image --> EB
    ES_Decision --> EB
    ES_System --> EB
    
    EB --> EH1
    EB --> EH2
    EB --> EH3
    EB --> EH4
    
    EH1 --> EC_Logging
    EH2 --> EC_Cache
    EH3 --> EC_Monitoring
    EH4 --> EC_Notification
    
    style EB fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
```

## 4. 数据流设计

### 4.1 脚本执行数据流

```mermaid
flowchart TD
    A[Start Script Execution] --> B[Load Script Configuration]
    B --> C[Initialize Execution Environment]
    C --> D[Start Sense Service]
    D --> E{Sense Service Ready?}
    
    E -->|Yes| F[Enter Main Loop]
    E -->|No| G[Error Recovery]
    G --> C
    
    F --> H[Capture Screen State]
    H --> I[Execute Image Recognition]
    I --> J{Recognition Success?}
    
    J -->|Yes| K[Update Perception Context]
    J -->|No| L[Retry/Fallback Strategy]
    L --> I
    
    K --> M[Judge Decision Making]
    M --> N{Decision Made?}
    
    N -->|Yes| O[Execute Actions]
    N -->|No| P[Skip Action]
    
    O --> Q{Execution Success?}
    Q -->|Yes| R[Update Execution Status]
    Q -->|No| S[Error Handling]
    
    S --> R
    R --> T{Continue Loop?}
    
    T -->|Yes| H
    T -->|No| U[Cleanup Resources]
    U --> V[End Execution]
    
    P --> T
    G --> V
```

### 4.2 图像识别数据流

```mermaid
flowchart TD
    A[Start Image Recognition] --> B[Load Template]
    B --> C[Configure Recognition Parameters]
    C --> D[Capture Screen Image]
    D --> E[Image Preprocessing]
    
    E --> F{Preprocessing Success?}
    F -->|Yes| G[Execute Recognition Algorithm]
    F -->|No| H[Use Original Image]
    H --> G
    
    G --> I[Calculate Match Confidence]
    I --> J{Confidence >= Threshold?}
    
    J -->|Yes| K[Generate Recognition Result]
    J -->|No| L[Try Alternative Algorithm]
    
    L --> M{Alternative Success?}
    M -->|Yes| K
    M -->|No| N[Mark as Failed]
    
    K --> O[Result Validation]
    O --> P{Validation Success?}
    
    P -->|Yes| Q[Return Success Result]
    P -->|No| R[Mark as Unreliable]
    
    R --> Q
    N --> S[Return Failed Result]
    
    Q --> T[End Recognition]
    S --> T
```

### 4.3 决策引擎数据流

```mermaid
flowchart TD
    A[Start Decision Making] --> B[Get Execution Context]
    B --> C[Load Active Rules]
    C --> D[Load State Machine State]
    
    D --> E[Evaluate Rules]
    E --> F{Rules Match?}
    
    F -->|Yes| G[Execute Rule Actions]
    F -->|No| H[Process State Machine]
    
    H --> I{State Transition Possible?}
    I -->|Yes| J[Execute State Transition]
    I -->|No| K[Use Default Action]
    
    G --> L[Combine Results]
    J --> L
    K --> L
    
    L --> M[Generate Decision Result]
    M --> N[Log Decision]
    N --> O[Publish Decision Event]
    O --> P[Return Decision Result]
    
    P --> Q[End Decision Making]
```

## 5. 部署架构图

### 5.1 容器化部署架构

```mermaid
graph TB
    subgraph "Docker Host"
        subgraph "KeyForge API Container"
            API1[API App]
            DB1[(SQLite)]
        end
        
        subgraph "KeyForge Desktop Container"
            DESKTOP1[Desktop App]
            CFG1[Config Files]
        end
        
        subgraph "Infrastructure Services"
            RABBITMQ[RabbitMQ]
            REDIS[Redis]
            MONITORING[Monitoring]
        end
        
        subgraph "External Services"
            DB_External[(External DB)]
            CLOUD[Cloud Services]
        end
    end
    
    API1 --> RABBITMQ
    API1 --> REDIS
    API1 --> DB1
    
    DESKTOP1 --> RABBITMQ
    DESKTOP1 --> REDIS
    
    RABBITMQ --> MONITORING
    REDIS --> MONITORING
    
    API1 -.-> DB_External
    API1 -.-> CLOUD
    
    style API1 fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style DESKTOP1 fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style RABBITMQ fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
    style REDIS fill:#fce4ec,stroke:#c2185b,stroke-width:2px
```

### 5.2 生产环境部署架构

```mermaid
graph TB
    subgraph "Load Balancer"
        LB[Load Balancer]
    end
    
    subgraph "Web Tier"
        API1[API Server 1]
        API2[API Server 2]
        API3[API Server 3]
    end
    
    subgraph "Application Tier"
        APP1[App Service 1]
        APP2[App Service 2]
    end
    
    subgraph "Data Tier"
        DB_PRIMARY[(Primary DB)]
        DB_REPLICA[(Replica DB)]
        REDIS_CLUSTER[Redis Cluster]
    end
    
    subgraph "Message Queue"
        MQ1[RabbitMQ 1]
        MQ2[RabbitMQ 2]
        MQ3[RabbitMQ 3]
    end
    
    subgraph "Monitoring"
        PROMETHEUS[Prometheus]
        GRAFANA[Grafana]
        ALERTMANAGER[AlertManager]
    end
    
    LB --> API1
    LB --> API2
    LB --> API3
    
    API1 --> APP1
    API2 --> APP1
    API3 --> APP2
    
    APP1 --> DB_PRIMARY
    APP2 --> DB_REPLICA
    
    APP1 --> REDIS_CLUSTER
    APP2 --> REDIS_CLUSTER
    
    APP1 --> MQ1
    APP2 --> MQ2
    
    MQ1 --> PROMETHEUS
    MQ2 --> PROMETHEUS
    MQ3 --> PROMETHEUS
    
    PROMETHEUS --> GRAFANA
    PROMETHEUS --> ALERTMANAGER
    
    style LB fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style DB_PRIMARY fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
    style REDIS_CLUSTER fill:#fce4ec,stroke:#c2185b,stroke-width:2px
```

## 6. 安全架构图

### 6.1 安全组件架构

```mermaid
graph TB
    subgraph "Security Layer"
        AUTH[Authentication Service]
        AUTHZ[Authorization Service]
        ENC[Encryption Service]
        AUDIT[Audit Service]
    end
    
    subgraph "Application Layer"
        API[Web API]
        UI[WPF UI]
        SERVICES[Background Services]
    end
    
    subgraph "Data Layer"
        DB[(Database)]
        FILES[File System]
        CACHE[(Cache)]
    end
    
    subgraph "External Services"
        EXTERNAL_API[External APIs]
        CLOUD_SERVICES[Cloud Services]
    end
    
    API --> AUTH
    API --> AUTHZ
    UI --> AUTH
    UI --> AUTHZ
    
    API --> ENC
    UI --> ENC
    SERVICES --> ENC
    
    API --> AUDIT
    UI --> AUDIT
    SERVICES --> AUDIT
    
    DB --> ENC
    FILES --> ENC
    CACHE --> ENC
    
    API --> EXTERNAL_API
    SERVICES --> CLOUD_SERVICES
    
    style AUTH fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style AUTHZ fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style ENC fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
    style AUDIT fill:#fce4ec,stroke:#c2185b,stroke-width:2px
```

### 6.2 数据加密流程

```mermaid
flowchart TD
    A[Sensitive Data] --> B[Encryption Service]
    B --> C{Encryption Required?}
    
    C -->|Yes| D[Generate Encryption Key]
    D --> E[Encrypt Data]
    E --> F[Store Encrypted Data]
    
    C -->|No| G[Store Plain Data]
    
    F --> H[Store Key Securely]
    G --> I[Access Control]
    
    J[Data Access Request] --> K[Authentication]
    K --> L{Authorized?}
    
    L -->|Yes| M[Retrieve Encryption Key]
    M --> N[Decrypt Data]
    N --> O[Return Decrypted Data]
    
    L -->|No| P[Access Denied]
    
    H --> M
    I --> L
```

## 7. 监控和日志架构

### 7.1 监控系统架构

```mermaid
graph TB
    subgraph "Application Monitoring"
        APP_METRICS[Application Metrics]
        PERF_COUNTERS[Performance Counters]
        HEALTH_CHECKS[Health Checks]
    end
    
    subgraph "Infrastructure Monitoring"
        SYSTEM_METRICS[System Metrics]
        NETWORK_METRICS[Network Metrics]
        DISK_METRICS[Disk Metrics]
    end
    
    subgraph "Log Collection"
        STRUCTURED_LOGS[Structured Logs]
        ERROR_LOGS[Error Logs]
        AUDIT_LOGS[Audit Logs]
    end
    
    subgraph "Monitoring Stack"
        PROMETHEUS[Prometheus]
        GRAFANA[Grafana]
        ELASTICSEARCH[Elasticsearch]
        KIBANA[Kibana]
    end
    
    subgraph "Alerting"
        ALERTMANAGER[AlertManager]
        NOTIFICATIONS[Notifications]
    end
    
    APP_METRICS --> PROMETHEUS
    PERF_COUNTERS --> PROMETHEUS
    HEALTH_CHECKS --> PROMETHEUS
    
    SYSTEM_METRICS --> PROMETHEUS
    NETWORK_METRICS --> PROMETHEUS
    DISK_METRICS --> PROMETHEUS
    
    STRUCTURED_LOGS --> ELASTICSEARCH
    ERROR_LOGS --> ELASTICSEARCH
    AUDIT_LOGS --> ELASTICSEARCH
    
    PROMETHEUS --> GRAFANA
    PROMETHEUS --> ALERTMANAGER
    
    ELASTICSEARCH --> KIBANA
    
    ALERTMANAGER --> NOTIFICATIONS
    
    style PROMETHEUS fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style ELASTICSEARCH fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style ALERTMANAGER fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
```

### 7.2 日志处理流程

```mermaid
flowchart TD
    A[Application Logs] --> B[Log Collection]
    B --> C[Log Parsing]
    C --> D[Log Enrichment]
    
    D --> E{Log Level}
    E -->|Debug| F[Debug Logs]
    E -->|Info| G[Info Logs]
    E -->|Warning| H[Warning Logs]
    E -->|Error| I[Error Logs]
    E -->|Fatal| J[Fatal Logs]
    
    F --> K[Log Storage]
    G --> K
    H --> K
    I --> K
    J --> K
    
    I --> L[Alert Generation]
    J --> L
    
    K --> M[Log Indexing]
    M --> N[Log Search]
    N --> O[Log Analysis]
    
    L --> P[Alert Notification]
    O --> Q[Performance Metrics]
    O --> R[Error Trends]
    
    style K fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style M fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style L fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
```

## 8. 扩展和插件架构

### 8.1 插件系统架构

```mermaid
graph TB
    subgraph "Plugin Host"
        HOST[Plugin Host]
        LOADER[Plugin Loader]
        MANAGER[Plugin Manager]
    end
    
    subgraph "Core Plugins"
        CORE1[Image Recognition Plugin]
        CORE2[Script Execution Plugin]
        CORE3[Decision Engine Plugin]
    end
    
    subgraph "Custom Plugins"
        CUSTOM1[Custom Algorithm Plugin]
        CUSTOM2[External Integration Plugin]
        CUSTOM3[Custom UI Plugin]
    end
    
    subgraph "Plugin SDK"
        SDK[Plugin SDK]
        APIS[Plugin APIs]
        DOCS[Documentation]
    end
    
    HOST --> LOADER
    HOST --> MANAGER
    LOADER --> SDK
    
    MANAGER --> CORE1
    MANAGER --> CORE2
    MANAGER --> CORE3
    MANAGER --> CUSTOM1
    MANAGER --> CUSTOM2
    MANAGER --> CUSTOM3
    
    SDK --> APIS
    SDK --> DOCS
    
    CUSTOM1 --> APIS
    CUSTOM2 --> APIS
    CUSTOM3 --> APIS
    
    style HOST fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style SDK fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style CUSTOM1 fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
```

### 8.2 扩展点架构

```mermaid
graph TB
    subgraph "Extension Points"
        EP1[Image Recognition Algorithms]
        EP2[Decision Logic Plugins]
        EP3[Action Executors]
        EP4[Data Sources]
        EP5[UI Components]
        EP6[Notification Channels]
    end
    
    subgraph "Core System"
        CORE[Core System]
        API[Extension API]
        EVENTS[Event System]
    end
    
    subgraph "Extensions"
        EXT1[Custom Algorithm]
        EXT2[ML Decision Plugin]
        EXT3[Hardware Control]
        EXT4[Cloud Integration]
        EXT5[Custom Dashboard]
        EXT6[Slack Notifications]
    end
    
    CORE --> API
    API --> EP1
    API --> EP2
    API --> EP3
    API --> EP4
    API --> EP5
    API --> EP6
    
    EVENTS --> EP1
    EVENTS --> EP2
    EVENTS --> EP3
    
    EXT1 --> EP1
    EXT2 --> EP2
    EXT3 --> EP3
    EXT4 --> EP4
    EXT5 --> EP5
    EXT6 --> EP6
    
    style CORE fill:#f9f,stroke:#333,stroke-width:2px
    style EP1 fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style EXT1 fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
```

## 9. 性能优化架构

### 9.1 性能优化组件

```mermaid
graph TB
    subgraph "Caching Layer"
        MEMORY_CACHE[Memory Cache]
        REDIS_CACHE[Redis Cache]
        CDN_CACHE[CDN Cache]
    end
    
    subgraph "Performance Optimization"
        CONNECTION_POOL[Connection Pool]
        ASYNC_PROCESSING[Async Processing]
        PARALLEL_EXECUTION[Parallel Execution]
        LAZY_LOADING[Lazy Loading]
    end
    
    subgraph "Monitoring & Optimization"
        PERF_MONITOR[Performance Monitor]
        QUERY_OPTIMIZER[Query Optimizer]
        RESOURCE_MANAGER[Resource Manager]
    end
    
    subgraph "Load Balancing"
        LOAD_BALANCER[Load Balancer]
        AUTO_SCALING[Auto Scaling]
        CIRCUIT_BREAKER[Circuit Breaker]
    end
    
    MEMORY_CACHE --> PERF_MONITOR
    REDIS_CACHE --> PERF_MONITOR
    CDN_CACHE --> PERF_MONITOR
    
    CONNECTION_POOL --> RESOURCE_MANAGER
    ASYNC_PROCESSING --> RESOURCE_MANAGER
    PARALLEL_EXECUTION --> RESOURCE_MANAGER
    LAZY_LOADING --> RESOURCE_MANAGER
    
    PERF_MONITOR --> QUERY_OPTIMIZER
    RESOURCE_MANAGER --> AUTO_SCALING
    
    LOAD_BALANCER --> CIRCUIT_BREAKER
    AUTO_SCALING --> LOAD_BALANCER
    
    style MEMORY_CACHE fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style ASYNC_PROCESSING fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style PERF_MONITOR fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
    style LOAD_BALANCER fill:#fce4ec,stroke:#c2185b,stroke-width:2px
```

### 9.2 查询优化流程

```mermaid
flowchart TD
    A[Query Request] --> B[Query Parser]
    B --> C[Query Analyzer]
    C --> D{Query Optimization Needed?}
    
    D -->|Yes| E[Generate Execution Plan]
    D -->|No| F[Execute Direct Query]
    
    E --> G[Optimize Execution Plan]
    G --> H[Execute Optimized Query]
    
    F --> I[Collect Performance Metrics]
    H --> I
    
    I --> J{Performance Acceptable?}
    
    J -->|Yes| K[Return Results]
    J -->|No| L[Adjust Optimization Strategy]
    
    L --> M[Update Query Cache]
    M --> C
    
    K --> N[Cache Results]
    N --> O[End Query]
    
    style E fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style G fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style L fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
```

## 10. 故障恢复架构

### 10.1 故障恢复组件

```mermaid
graph TB
    subgraph "Fault Detection"
        HEALTH_CHECKS[Health Checks]
        ERROR_MONITORING[Error Monitoring]
        PERFORMANCE_ALERTS[Performance Alerts]
    end
    
    subgraph "Fault Recovery"
        RETRY_MECHANISM[Retry Mechanism]
        CIRCUIT_BREAKER[Circuit Breaker]
        FALLBACK_STRATEGY[Fallback Strategy]
        AUTO_RECOVERY[Auto Recovery]
    end
    
    subgraph "Data Protection"
        BACKUP_SERVICE[Backup Service]
        DATA_REPLICATION[Data Replication]
        TRANSACTION_MANAGER[Transaction Manager]
    end
    
    subgraph "Notification & Escalation"
        ALERT_SERVICE[Alert Service]
        ESCALATION_SERVICE[Escalation Service]
        NOTIFICATION_CHANNELS[Notification Channels]
    end
    
    HEALTH_CHECKS --> ERROR_MONITORING
    ERROR_MONITORING --> PERFORMANCE_ALERTS
    
    ERROR_MONITORING --> RETRY_MECHANISM
    PERFORMANCE_ALERTS --> CIRCUIT_BREAKER
    
    RETRY_MECHANISM --> FALLBACK_STRATEGY
    CIRCUIT_BREAKER --> AUTO_RECOVERY
    
    FALLBACK_STRATEGY --> BACKUP_SERVICE
    AUTO_RECOVERY --> DATA_REPLICATION
    
    BACKUP_SERVICE --> TRANSACTION_MANAGER
    DATA_REPLICATION --> TRANSACTION_MANAGER
    
    ERROR_MONITORING --> ALERT_SERVICE
    PERFORMANCE_ALERTS --> ESCALATION_SERVICE
    
    ALERT_SERVICE --> NOTIFICATION_CHANNELS
    ESCALATION_SERVICE --> NOTIFICATION_CHANNELS
    
    style HEALTH_CHECKS fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style RETRY_MECHANISM fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style BACKUP_SERVICE fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
    style ALERT_SERVICE fill:#fce4ec,stroke:#c2185b,stroke-width:2px
```

### 10.2 故障恢复流程

```mermaid
flowchart TD
    A[System Running] --> B[Fault Detected]
    B --> C{Fault Type?}
    
    C -->|Hardware Failure| D[Hardware Recovery]
    C -->|Software Failure| E[Software Recovery]
    C -->|Network Failure| F[Network Recovery]
    C -->|Data Corruption| G[Data Recovery]
    
    D --> H[Replace Hardware]
    E --> I[Restart Service]
    F --> J[Switch Network]
    G --> K[Restore Backup]
    
    H --> L[Test Recovery]
    I --> L
    J --> L
    K --> L
    
    L --> M{Recovery Successful?}
    
    M -->|Yes| N[Resume Normal Operations]
    M -->|No| O[Escalate to Manual Recovery]
    
    N --> P[Update System Status]
    O --> Q[Notify Administrators]
    
    P --> R[End Recovery Process]
    Q --> S[Manual Intervention]
    
    style D fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style E fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    style F fill:#fff3e0,stroke:#ef6c00,stroke-width:2px
    style G fill:#fce4ec,stroke:#c2185b,stroke-width:2px
```

## 11. 总结

KeyForge系统的架构设计充分考虑了系统的复杂性、可扩展性和可维护性。通过这些架构图，我们可以清晰地看到系统的各个组件、它们之间的关系以及数据流。

### 11.1 架构亮点
- **清晰的分层架构**：Clean Architecture确保了系统的可维护性
- **领域驱动设计**：DDD模式使业务逻辑更加清晰
- **Sense-Judge-Act闭环**：智能化的决策和执行流程
- **模块化设计**：高度模块化的组件便于扩展和维护
- **完整的监控体系**：全方位的系统监控和故障恢复

### 11.2 技术优势
- **现代化技术栈**：基于.NET 6.0+的最新技术
- **高性能**：优化的图像识别和执行性能
- **高可用性**：完整的故障恢复和备份机制
- **可扩展性**：支持插件和第三方扩展
- **安全性**：完整的安全架构和数据保护

### 11.3 实施建议
- **分阶段实施**：按照优先级分阶段实施各个模块
- **持续优化**：根据实际使用情况持续优化系统性能
- **监控告警**：建立完整的监控和告警体系
- **文档维护**：保持架构文档的及时更新
- **团队培训**：确保团队理解架构设计理念

这些架构图为KeyForge系统的开发和维护提供了清晰的指导，帮助团队理解系统的整体结构和各个组件之间的关系。