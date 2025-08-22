openapi: 3.0.0
info:
  title: KeyForge 跨平台 API 规范 v2.0
  version: 2.0.0
  description: KeyForge按键脚本系统的跨平台API接口规范，支持Windows、macOS、Linux平台，集成了质量监控和管理功能

servers:
  - url: http://localhost:5000/api
    description: 本地开发服务器
  - url: https://api.keyforge.com/v2
    description: 生产服务器

paths:
  # 系统管理
  /system/info:
    get:
      summary: 获取系统信息
      operationId: getSystemInfo
      tags:
        - 系统
      responses:
        '200':
          description: 系统信息
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/SystemInfo'

  /system/health:
    get:
      summary: 健康检查
      operationId: getSystemHealth
      tags:
        - 系统
      responses:
        '200':
          description: 健康状态
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/HealthCheckResult'

  /system/platform:
    get:
      summary: 获取平台信息
      operationId: getPlatformInfo
      tags:
        - 系统
      responses:
        '200':
          description: 平台信息
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PlatformInfo'

  # 性能监控
  /monitoring/metrics:
    get:
      summary: 获取性能指标
      operationId: getPerformanceMetrics
      tags:
        - 性能监控
      parameters:
        - name: startTime
          in: query
          schema:
            type: string
            format: date-time
        - name: endTime
          in: query
          schema:
            type: string
            format: date-time
        - name: interval
          in: query
          schema:
            type: string
            enum: [1m, 5m, 15m, 1h, 1d]
            default: 5m
      responses:
        '200':
          description: 性能指标数据
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/PerformanceMetrics'

  /monitoring/benchmark:
    post:
      summary: 运行性能基准测试
      operationId: runBenchmark
      tags:
        - 性能监控
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/BenchmarkRequest'
      responses:
        '200':
          description: 基准测试结果
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/BenchmarkResult'

  /monitoring/alerts:
    get:
      summary: 获取告警信息
      operationId: getAlerts
      tags:
        - 性能监控
      parameters:
        - name: severity
          in: query
          schema:
            type: string
            enum: [Critical, Warning, Info]
        - name: status
          in: query
          schema:
            type: string
            enum: [Active, Acknowledged, Resolved]
        - name: limit
          in: query
          schema:
            type: integer
            default: 100
      responses:
        '200':
          description: 告警列表
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/PerformanceAlert'

  # 质量门禁
  /quality/gates:
    get:
      summary: 获取质量门禁状态
      operationId: getQualityGates
      tags:
        - 质量门禁
      responses:
        '200':
          description: 质量门禁状态
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/QualityGateStatus'

  /quality/evaluate:
    post:
      summary: 执行质量评估
      operationId: evaluateQuality
      tags:
        - 质量门禁
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/QualityEvaluationRequest'
      responses:
        '200':
          description: 质量评估结果
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/QualityEvaluationResult'

  /quality/report:
    get:
      summary: 生成质量报告
      operationId: generateQualityReport
      tags:
        - 质量门禁
      parameters:
        - name: format
          in: query
          schema:
            type: string
            enum: [json, html, pdf]
            default: json
        - name: includeDetails
          in: query
          schema:
            type: boolean
            default: true
      responses:
        '200':
          description: 质量报告
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/QualityReport'

  # 脚本管理
  /scripts:
    get:
      summary: 获取脚本列表
      operationId: getScripts
      tags:
        - 脚本管理
      parameters:
        - name: page
          in: query
          schema:
            type: integer
            default: 1
        - name: limit
          in: query
          schema:
            type: integer
            default: 20
        - name: type
          in: query
          schema:
            $ref: '#/components/schemas/ScriptType'
        - name: platform
          in: query
          schema:
            $ref: '#/components/schemas/Platform'
        - name: status
          in: query
          schema:
            $ref: '#/components/schemas/ScriptStatus'
      responses:
        '200':
          description: 脚本列表
          content:
            application/json:
              schema:
                type: object
                properties:
                  scripts:
                    type: array
                    items:
                      $ref: '#/components/schemas/Script'
                  pagination:
                    $ref: '#/components/schemas/Pagination'

    post:
      summary: 创建脚本
      operationId: createScript
      tags:
        - 脚本管理
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CreateScriptRequest'
      responses:
        '201':
          description: 创建成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Script'

  /scripts/{id}:
    get:
      summary: 获取脚本详情
      operationId: getScript
      tags:
        - 脚本管理
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        '200':
          description: 脚本详情
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Script'
        '404':
          description: 脚本不存在

    put:
      summary: 更新脚本
      operationId: updateScript
      tags:
        - 脚本管理
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/UpdateScriptRequest'
      responses:
        '200':
          description: 更新成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Script'

    delete:
      summary: 删除脚本
      operationId: deleteScript
      tags:
        - 脚本管理
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        '204':
          description: 删除成功

  # 脚本执行
  /scripts/{id}/execute:
    post:
      summary: 执行脚本
      operationId: executeScript
      tags:
        - 脚本执行
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ExecuteScriptRequest'
      responses:
        '200':
          description: 执行成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ExecutionResult'

  /scripts/{id}/stop:
    post:
      summary: 停止脚本执行
      operationId: stopScript
      tags:
        - 脚本执行
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        '200':
          description: 停止成功

  /scripts/{id}/status:
    get:
      summary: 获取脚本执行状态
      operationId: getScriptExecutionStatus
      tags:
        - 脚本执行
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        '200':
          description: 执行状态
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ExecutionStatus'

  # 图像识别
  /images/capture:
    post:
      summary: 截取屏幕图像
      operationId: captureScreen
      tags:
        - 图像识别
      requestBody:
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CaptureScreenRequest'
      responses:
        '200':
          description: 截图成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/CaptureScreenResult'

  /images/find:
    post:
      summary: 查找图像
      operationId: findImage
      tags:
        - 图像识别
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/FindImageRequest'
      responses:
        '200':
          description: 查找结果
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/FindImageResult'

  # 全局快捷键
  /hotkeys:
    get:
      summary: 获取全局快捷键列表
      operationId: getHotkeys
      tags:
        - 全局快捷键
      responses:
        '200':
          description: 快捷键列表
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/Hotkey'

    post:
      summary: 注册全局快捷键
      operationId: registerHotkey
      tags:
        - 全局快捷键
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/RegisterHotkeyRequest'
      responses:
        '201':
          description: 注册成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Hotkey'

  /hotkeys/{id}:
    delete:
      summary: 注销全局快捷键
      operationId: unregisterHotkey
      tags:
        - 全局快捷键
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        '204':
          description: 注销成功

  # 设备控制
  /devices/keyboard:
    post:
      summary: 键盘操作
      operationId: keyboardAction
      tags:
        - 设备控制
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/KeyboardActionRequest'
      responses:
        '200':
          description: 操作成功

  /devices/mouse:
    post:
      summary: 鼠标操作
      operationId: mouseAction
      tags:
        - 设备控制
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/MouseActionRequest'
      responses:
        '200':
          description: 操作成功

  # 权限管理
  /permissions:
    get:
      summary: 获取权限状态
      operationId: getPermissions
      tags:
        - 权限管理
      responses:
        '200':
          description: 权限状态
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PermissionStatus'

    post:
      summary: 请求权限
      operationId: requestPermissions
      tags:
        - 权限管理
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/PermissionRequest'
      responses:
        '200':
          description: 请求结果
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PermissionResult'

  # 日志管理
  /logs:
    get:
      summary: 获取日志
      operationId: getLogs
      tags:
        - 日志管理
      parameters:
        - name: level
          in: query
          schema:
            type: string
            enum: [Debug, Information, Warning, Error, Critical]
        - name: startTime
          in: query
          schema:
            type: string
            format: date-time
        - name: endTime
          in: query
          schema:
            type: string
            format: date-time
        - name: source
          in: query
          schema:
            type: string
        - name: limit
          in: query
          schema:
            type: integer
            default: 1000
      responses:
        '200':
          description: 日志列表
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/LogEntry'

  # 配置管理
  /configuration:
    get:
      summary: 获取配置
      operationId: getConfiguration
      tags:
        - 配置管理
      responses:
        '200':
          description: 配置信息
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Configuration'

    put:
      summary: 更新配置
      operationId: updateConfiguration
      tags:
        - 配置管理
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/Configuration'
      responses:
        '200':
          description: 更新成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Configuration'

components:
  schemas:
    # 基础类型
    SystemInfo:
      type: object
      properties:
        version:
          type: string
        platform:
          $ref: '#/components/schemas/Platform'
        architecture:
          type: string
        uptime:
          type: integer
          format: int64
        memory:
          type: object
          properties:
            total:
              type: integer
              format: int64
            used:
              type: integer
              format: int64
            free:
              type: integer
              format: int64
        buildNumber:
          type: string
        environment:
          type: string
          enum: [Development, Staging, Production]

    HealthCheckResult:
      type: object
      properties:
        status:
          type: string
          enum: [Healthy, Degraded, Unhealthy]
        checks:
          type: array
          items:
            $ref: '#/components/schemas/HealthCheck'
        timestamp:
          type: string
          format: date-time
        duration:
          type: integer
          format: int64

    HealthCheck:
      type: object
      properties:
        name:
          type: string
        status:
          type: string
          enum: [Healthy, Degraded, Unhealthy]
        description:
          type: string
        duration:
          type: integer
          format: int64
        data:
          type: object

    PlatformInfo:
      type: object
      properties:
        name:
          type: string
        version:
          type: string
        features:
          type: array
          items:
            type: string
        capabilities:
          $ref: '#/components/schemas/PlatformCapabilities'

    Platform:
      type: string
      enum:
        - Windows
        - MacOS
        - Linux
        - Unknown

    PlatformCapabilities:
      type: object
      properties:
        globalHotkeys:
          type: boolean
        screenCapture:
          type: boolean
        inputInjection:
          type: boolean
        windowControl:
          type: boolean
        imageRecognition:
          type: boolean
        performanceMonitoring:
          type: boolean

    # 性能监控相关
    PerformanceMetrics:
      type: object
      properties:
        timestamp:
          type: string
          format: date-time
        cpuUsage:
          type: number
          format: double
        memoryUsage:
          type: number
          format: double
        diskUsage:
          type: number
          format: double
        networkUsage:
          type: number
          format: double
        customMetrics:
          type: object
          additionalProperties:
            type: number
            format: double
        tags:
          type: object
          additionalProperties:
            type: string

    BenchmarkRequest:
      type: object
      properties:
        testType:
          type: string
          enum: [Cpu, Memory, Disk, Network, Custom]
        duration:
          type: integer
          format: int32
          default: 10000
        iterations:
          type: integer
          format: int32
          default: 100
        parameters:
          type: object

    BenchmarkResult:
      type: object
      properties:
        testType:
          type: string
        startTime:
          type: string
          format: date-time
        endTime:
          type: string
          format: date-time
        duration:
          type: integer
          format: int64
        iterations:
          type: integer
          format: int32
        averageTime:
          type: number
          format: double
        minTime:
          type: number
          format: double
        maxTime:
          type: number
          format: double
        standardDeviation:
          type: number
          format: double
        throughput:
          type: number
          format: double
        success:
          type: boolean
        error:
          type: string

    PerformanceAlert:
      type: object
      properties:
        id:
          type: string
          format: uuid
        timestamp:
          type: string
          format: date-time
        severity:
          type: string
          enum: [Critical, Warning, Info]
        type:
          type: string
        message:
          type: string
        metricName:
          type: string
        currentValue:
          type: number
          format: double
        threshold:
          type: number
          format: double
        status:
          type: string
          enum: [Active, Acknowledged, Resolved]
        acknowledgedAt:
          type: string
          format: date-time
        resolvedAt:
          type: string
          format: date-time

    # 质量门禁相关
    QualityGateStatus:
      type: object
      properties:
        overallStatus:
          type: string
          enum: [Passed, Failed, Warning]
        score:
          type: number
          format: double
        gates:
          type: array
          items:
            $ref: '#/components/schemas/QualityGateResult'
        lastUpdated:
          type: string
          format: date-time

    QualityGateResult:
      type: object
      properties:
        gateName:
          type: string
        gateType:
          type: string
          enum: [Compilation, TestCoverage, CodeQuality, Performance, Security]
        status:
          type: string
          enum: [Passed, Failed, Warning]
        score:
          type: number
          format: double
        issues:
          type: array
          items:
            $ref: '#/components/schemas/QualityIssue'
        details:
          type: object

    QualityIssue:
      type: object
      properties:
        type:
          type: string
          enum: [CompilationError, CompilationWarning, TestFailure, CodeQuality, Performance, Security]
        severity:
          type: string
          enum: [Critical, Warning, Info]
        message:
          type: string
        location:
          type: string
        rule:
          type: string
        suggestion:
          type: string

    QualityEvaluationRequest:
      type: object
      properties:
        includeCompilation:
          type: boolean
          default: true
        includeTests:
          type: boolean
          default: true
        includeCodeQuality:
          type: boolean
          default: true
        includePerformance:
          type: boolean
          default: true
        includeSecurity:
          type: boolean
          default: true

    QualityEvaluationResult:
      type: object
      properties:
        overallScore:
          type: number
          format: double
        status:
          type: string
          enum: [Passed, Failed, Warning]
        results:
          type: array
          items:
            $ref: '#/components/schemas/QualityGateResult'
        recommendations:
          type: array
          items:
            type: string
        timestamp:
          type: string
          format: date-time

    QualityReport:
      type: object
      properties:
        id:
          type: string
          format: uuid
        generatedAt:
          type: string
          format: date-time
        projectName:
          type: string
        version:
          type: string
        platform:
          $ref: '#/components/schemas/Platform'
        overallScore:
          type: number
          format: double
        status:
          type: string
          enum: [Excellent, Good, Fair, Poor]
        summary:
          type: string
        sections:
          type: array
          items:
            $ref: '#/components/schemas/ReportSection'
        recommendations:
          type: array
          items:
            type: string

    ReportSection:
      type: object
      properties:
        title:
          type: string
        content:
          type: string
        metrics:
          type: object
        charts:
          type: array
          items:
            type: object

    # 脚本相关
    Script:
      type: object
      properties:
        id:
          type: string
          format: uuid
        name:
          type: string
        description:
          type: string
        type:
          $ref: '#/components/schemas/ScriptType'
        actions:
          type: array
          items:
            $ref: '#/components/schemas/ScriptAction'
        status:
          $ref: '#/components/schemas/ScriptStatus'
        createdAt:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time
        platform:
          $ref: '#/components/schemas/Platform'
        compatibility:
          $ref: '#/components/schemas/PlatformCompatibility'
        metadata:
          $ref: '#/components/schemas/ScriptMetadata'

    ScriptType:
      type: string
      enum:
        - Sequence
        - Conditional
        - Loop
        - StateMachine
        - Hybrid

    ScriptAction:
      type: object
      properties:
        id:
          type: string
        type:
          $ref: '#/components/schemas/ActionType'
        parameters:
          type: object
        delay:
          type: integer
          default: 0
        condition:
          $ref: '#/components/schemas/ExecutionCondition'
        platform:
          $ref: '#/components/schemas/Platform'
        compatibility:
          $ref: '#/components/schemas/PlatformCompatibility'

    ActionType:
      type: string
      enum:
        - KeyPress
        - KeyRelease
        - KeyType
        - MouseMove
        - MouseClick
        - MouseScroll
        - ImageFind
        - ImageWait
        - ColorCheck
        - Delay
        - SoundPlay
        - WindowActivate
        - WindowClose
        - ProcessStart
        - ProcessStop

    ScriptStatus:
      type: string
      enum:
        - Draft
        - Active
        - Inactive
        - Running
        - Paused
        - Error

    ScriptMetadata:
      type: object
      properties:
        author:
          type: string
        tags:
          type: array
          items:
            type: string
        version:
          type: string
        executionCount:
          type: integer
          format: int64
        lastExecuted:
          type: string
          format: date-time
        averageExecutionTime:
          type: integer
          format: int64

    PlatformCompatibility:
      type: object
      properties:
        windows:
          type: boolean
        macOS:
          type: boolean
        linux:
          type: boolean
        notes:
          type: array
          items:
            type: string

    ExecutionCondition:
      type: object
      properties:
        type:
          type: string
          enum:
            - ImageFound
            - ColorMatch
            - WindowActive
            - ProcessRunning
            - TimeElapsed
            - KeyPressed
            - Custom
        parameters:
          type: object
        operator:
          type: string
          enum:
            - Equals
            - NotEquals
            - GreaterThan
            - LessThan
            - Contains
            - Matches

    # 请求/响应对象
    CreateScriptRequest:
      type: object
      required:
        - name
        - type
      properties:
        name:
          type: string
        description:
          type: string
        type:
          $ref: '#/components/schemas/ScriptType'
        actions:
          type: array
          items:
            $ref: '#/components/schemas/ScriptAction'
        platform:
          $ref: '#/components/schemas/Platform'

    UpdateScriptRequest:
      type: object
      properties:
        name:
          type: string
        description:
          type: string
        actions:
          type: array
          items:
            $ref: '#/components/schemas/ScriptAction'
        status:
          $ref: '#/components/schemas/ScriptStatus'

    ExecuteScriptRequest:
      type: object
      properties:
        parameters:
          type: object
        timeout:
          type: integer
          default: 30000
        async:
          type: boolean
          default: false
        enableLogging:
          type: boolean
          default: true

    ExecutionResult:
      type: object
      properties:
        success:
          type: boolean
        executionId:
          type: string
          format: uuid
        startTime:
          type: string
          format: date-time
        endTime:
          type: string
          format: date-time
        duration:
          type: integer
          format: int64
        error:
          type: string
        output:
          type: object
        logs:
          type: array
          items:
            $ref: '#/components/schemas/LogEntry'

    ExecutionStatus:
      type: object
      properties:
        executionId:
          type: string
          format: uuid
        status:
          type: string
          enum: [Running, Paused, Completed, Failed, Cancelled]
        progress:
          type: integer
          format: int32
        startTime:
          type: string
          format: date-time
        endTime:
          type: string
          format: date-time
        duration:
          type: integer
          format: int64
        currentAction:
          type: string
        actionsCompleted:
          type: integer
          format: int32
        totalActions:
          type: integer
          format: int32

    # 图像识别相关
    CaptureScreenRequest:
      type: object
      properties:
        region:
          $ref: '#/components/schemas/Rectangle'
        format:
          type: string
          enum:
            - PNG
            - JPEG
            - BMP
          default: PNG
        quality:
          type: integer
          minimum: 1
          maximum: 100
          default: 90
        screenIndex:
          type: integer
          default: -1

    CaptureScreenResult:
      type: object
      properties:
        success:
          type: boolean
        imageData:
          type: string
          format: base64
        size:
          $ref: '#/components/schemas/Size'
        format:
          type: string
        captureTime:
          type: integer
          format: int64

    FindImageRequest:
      type: object
      required:
        - templatePath
      properties:
        templatePath:
          type: string
        searchArea:
          $ref: '#/components/schemas/Rectangle'
        threshold:
          type: number
          minimum: 0
          maximum: 1
          default: 0.8
        findAll:
          type: boolean
          default: false
        engine:
          type: string
          enum: [OpenCV, ImageSharp]
          default: OpenCV

    FindImageResult:
      type: object
      properties:
        success:
          type: boolean
        matches:
          type: array
          items:
            $ref: '#/components/schemas/ImageMatch'
        confidence:
          type: number
        processingTime:
          type: integer
          format: int64
        engine:
          type: string

    ImageMatch:
      type: object
      properties:
        location:
          $ref: '#/components/schemas/Point'
        center:
          $ref: '#/components/schemas/Point'
        confidence:
          type: number
        size:
          $ref: '#/components/schemas/Size'

    # 快捷键相关
    Hotkey:
      type: object
      properties:
        id:
          type: string
          format: uuid
        key:
          type: string
        modifiers:
          type: array
          items:
            type: string
        action:
          type: string
        scriptId:
          type: string
          format: uuid
        isActive:
          type: boolean
        createdAt:
          type: string
          format: date-time
        lastTriggered:
          type: string
          format: date-time

    RegisterHotkeyRequest:
      type: object
      required:
        - key
        - action
      properties:
        key:
          type: string
        modifiers:
          type: array
          items:
            type: string
        action:
          type: string
        scriptId:
          type: string
          format: uuid

    # 设备控制相关
    KeyboardActionRequest:
      type: object
      required:
        - action
      properties:
        action:
          type: string
          enum:
            - Press
            - Release
            - Type
        key:
          type: string
        modifiers:
          type: array
          items:
            type: string
        text:
          type: string
        delay:
          type: integer
          default: 10

    MouseActionRequest:
      type: object
      required:
        - action
      properties:
        action:
          type: string
          enum:
            - Move
            - MoveRelative
            - Click
            - Press
            - Release
            - Scroll
        button:
          type: string
          enum:
            - Left
            - Right
            - Middle
            - X1
            - X2
        x:
          type: integer
        y:
          type: integer
        deltaX:
          type: integer
        deltaY:
          type: integer
        delta:
          type: integer
        count:
          type: integer
          default: 1

    # 权限相关
    PermissionStatus:
      type: object
      properties:
        accessibility:
          type: boolean
        screenCapture:
          type: boolean
        inputMonitoring:
          type: boolean
        automation:
          type: boolean
        administrator:
          type: boolean
        notes:
          type: array
          items:
            type: string

    PermissionRequest:
      type: object
      properties:
        permissions:
          type: array
          items:
            type: string
            enum:
              - Accessibility
              - ScreenCapture
              - InputMonitoring
              - Automation
              - Administrator

    PermissionResult:
      type: object
      properties:
        success:
          type: boolean
        granted:
          type: array
          items:
            type: string
        denied:
          type: array
          items:
            type: string
        requiresRestart:
          type: boolean
        instructions:
          type: string

    # 日志相关
    LogEntry:
      type: object
      properties:
        timestamp:
          type: string
          format: date-time
        level:
          type: string
          enum: [Debug, Information, Warning, Error, Critical]
        message:
          type: string
        source:
          type: string
        exception:
          type: string
        properties:
          type: object
          additionalProperties:
            type: string

    # 配置相关
    Configuration:
      type: object
      properties:
        general:
          type: object
          properties:
            language:
              type: string
            theme:
              type: string
            autoStart:
              type: boolean
        performance:
          type: object
          properties:
            enableCaching:
              type: boolean
            maxMemoryUsage:
              type: integer
              format: int64
            threadPoolSize:
              type: integer
        logging:
          type: object
          properties:
            level:
              type: string
            enableFileLogging:
              type: boolean
            maxLogSize:
              type: integer
              format: int64
        security:
          type: object
          properties:
            requireAdmin:
              type: boolean
            enableEncryption:
              type: boolean
        monitoring:
          type: object
          properties:
            enablePerformanceMonitoring:
              type: boolean
            metricsInterval:
              type: integer
            enableAlerts:
              type: boolean

    # 几何类型
    Point:
      type: object
      properties:
        x:
          type: integer
        y:
          type: integer

    Size:
      type: object
      properties:
        width:
          type: integer
        height:
          type: integer

    Rectangle:
      type: object
      properties:
        x:
          type: integer
        y:
          type: integer
        width:
          type: integer
        height:
          type: integer

    # 分页相关
    Pagination:
      type: object
      properties:
        page:
          type: integer
        limit:
          type: integer
        total:
          type: integer
        totalPages:
          type: integer

  # 安全方案
  securitySchemes:
    BearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
    ApiKeyAuth:
      type: apiKey
      in: header
      name: X-API-Key

security:
  - BearerAuth: []
  - ApiKeyAuth: []

tags:
  - name: 系统
    description: 系统信息和状态管理
  - name: 性能监控
    description: 性能指标监控和基准测试
  - name: 质量门禁
    description: 代码质量和测试覆盖率管理
  - name: 脚本管理
    description: 脚本的创建、读取、更新、删除
  - name: 脚本执行
    description: 脚本执行控制
  - name: 图像识别
    description: 图像识别和屏幕截图
  - name: 全局快捷键
    description: 全局快捷键管理
  - name: 设备控制
    description: 键盘鼠标设备控制
  - name: 权限管理
    description: 系统权限管理
  - name: 日志管理
    description: 日志查询和管理
  - name: 配置管理
    description: 系统配置管理