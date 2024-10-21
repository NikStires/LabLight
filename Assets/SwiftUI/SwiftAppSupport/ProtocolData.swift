import Foundation

// MARK: - Protocol Definition

struct ProtocolDefinition: Codable, Identifiable, Equatable, Hashable {
    let jsonId: String?
    let version: Int
    let title: String
    let pdfPath: String?
    let globalArElements: [ArElement]
    let steps: [Step]
    let mediaBasePath: String?
    
    // Computed property for Identifiable
    var id: String {
        jsonId ?? UUID().uuidString
    }
    
    // MARK: Coding Keys
    enum CodingKeys: String, CodingKey {
        case jsonId = "$id"
        case version, title, pdfPath, globalArElements, steps, mediaBasePath
    }
}

// MARK: - Step

struct Step: Codable, Identifiable {
    // MARK: Properties
    let jsonId: String?
    let isCritical: Bool
    let arElements: [ArElement]
    let checklist: [ChecklistItem]
    let signedOff: Bool?
    
    // Computed property for Identifiable
    var id: String {
        jsonId ?? UUID().uuidString
    }
    
    // MARK: Coding Keys
    enum CodingKeys: String, CodingKey {
        case jsonId = "$id"
        case isCritical, arElements, checklist
        case signedOff = "SignedOff"
    }
    
    // Custom initializer to provide default values
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        jsonId = try container.decodeIfPresent(String.self, forKey: .jsonId)
        isCritical = try container.decodeIfPresent(Bool.self, forKey: .isCritical) ?? false
        arElements = try container.decodeIfPresent([ArElement].self, forKey: .arElements) ?? []
        checklist = try container.decodeIfPresent([ChecklistItem].self, forKey: .checklist) ?? []
        signedOff = try container.decodeIfPresent(Bool.self, forKey: .signedOff)
    }
}

// MARK: - AR Element

struct ArElement: Codable, Identifiable, Equatable, Hashable {
    let jsonId: String?
    let layout: Layout?
    let arDefinitionType: String?
    let target: String?
    let count: Int?
    let isReadOnly: Bool?
    
    // Computed property for Identifiable
    var id: String {
        jsonId ?? UUID().uuidString
    }
    
    // MARK: Coding Keys
    enum CodingKeys: String, CodingKey {
        case jsonId = "$id"
        case layout, arDefinitionType, target
        case count = "Count"
        case isReadOnly = "IsReadOnly"
    }
    
    // Custom initializer to provide default values
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        jsonId = try container.decodeIfPresent(String.self, forKey: .jsonId)
        layout = try container.decodeIfPresent(Layout.self, forKey: .layout)
        arDefinitionType = try container.decodeIfPresent(String.self, forKey: .arDefinitionType)
        target = try container.decodeIfPresent(String.self, forKey: .target)
        count = try container.decodeIfPresent(Int.self, forKey: .count)
        isReadOnly = try container.decodeIfPresent(Bool.self, forKey: .isReadOnly)
    }
}

// MARK: - Layout

struct Layout: Codable, Identifiable, Equatable, Hashable {
    let jsonId: String?
    let layoutType: String?
    let contentItems: [ContentItem]?
    let contentType: String?
    
    // Computed property for Identifiable
    var id: String {
        jsonId ?? UUID().uuidString
    }
    
    // MARK: Coding Keys
    enum CodingKeys: String, CodingKey {
        case jsonId = "$id"
        case layoutType, contentItems, contentType
    }
    
    // Custom initializer to provide default values
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        jsonId = try container.decodeIfPresent(String.self, forKey: .jsonId)
        layoutType = try container.decodeIfPresent(String.self, forKey: .layoutType)
        contentItems = try container.decodeIfPresent([ContentItem].self, forKey: .contentItems)
        contentType = try container.decodeIfPresent(String.self, forKey: .contentType)
    }
}

// MARK: - Content Item

struct ContentItem: Codable, Identifiable, Equatable, Hashable {
    let jsonId: String?
    let textType: String?
    let text: String?
    let fontsize: Int?
    let contentType: String?
    
    // Computed property for Identifiable
    var id: String {
        jsonId ?? UUID().uuidString
    }
    
    // MARK: Coding Keys
    enum CodingKeys: String, CodingKey {
        case jsonId = "$id"
        case textType, text, fontsize, contentType
    }
    
    // Custom initializer to provide default values
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        jsonId = try container.decodeIfPresent(String.self, forKey: .jsonId)
        textType = try container.decodeIfPresent(String.self, forKey: .textType)
        text = try container.decodeIfPresent(String.self, forKey: .text)
        fontsize = try container.decodeIfPresent(Int.self, forKey: .fontsize)
        contentType = try container.decodeIfPresent(String.self, forKey: .contentType)
    }
}

// MARK: - Checklist Item

struct ChecklistItem: Codable, Identifiable, Equatable, Hashable {
    let jsonId: String?
    let text: String
    let activateTimer: Bool
    let hours: Int
    let minutes: Int
    let seconds: Int
    let completionTime: String?
    let isChecked: IsChecked?
    let arElements: [ArElement]
    let operations: [Operation]
    
    // Computed property for Identifiable
    var id: String {
        jsonId ?? UUID().uuidString
    }
    
    // MARK: Coding Keys
    enum CodingKeys: String, CodingKey {
        case jsonId = "$id"
        case text = "Text"
        case activateTimer, hours, minutes, seconds
        case completionTime = "CompletionTime"
        case isChecked = "IsChecked"
        case arElements, operations
    }
    
    // Custom initializer to provide default values
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        jsonId = try container.decodeIfPresent(String.self, forKey: .jsonId)
        text = try container.decode(String.self, forKey: .text)
        activateTimer = try container.decodeIfPresent(Bool.self, forKey: .activateTimer) ?? false
        hours = try container.decodeIfPresent(Int.self, forKey: .hours) ?? 0
        minutes = try container.decodeIfPresent(Int.self, forKey: .minutes) ?? 0
        seconds = try container.decodeIfPresent(Int.self, forKey: .seconds) ?? 0
        completionTime = try container.decodeIfPresent(String.self, forKey: .completionTime)
        isChecked = try container.decodeIfPresent(IsChecked.self, forKey: .isChecked)
        arElements = try container.decodeIfPresent([ArElement].self, forKey: .arElements) ?? []
        operations = try container.decodeIfPresent([Operation].self, forKey: .operations) ?? []
    }
}

// MARK: - Is Checked

struct IsChecked: Codable, Identifiable, Equatable, Hashable {
    let jsonId: String?
    let value: Bool?
    let hasValue: Bool?
    
    // Computed property for Identifiable
    var id: String {
        jsonId ?? UUID().uuidString
    }
    
    // MARK: Coding Keyss
    enum CodingKeys: String, CodingKey {
        case jsonId = "$id"
        case value = "Value"
        case hasValue = "HasValue"
    }
    
    // Custom initializer to provide default values
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        jsonId = try container.decodeIfPresent(String.self, forKey: .jsonId)
        value = try container.decodeIfPresent(Bool.self, forKey: .value)
        hasValue = try container.decodeIfPresent(Bool.self, forKey: .hasValue)
    }
}

// MARK: - Operation

struct Operation: Codable, Identifiable, Equatable, Hashable {
    let jsonId: String?
    let type: String?
    let details: String?
    
    // Computed property for Identifiable
    var id: String {
        jsonId ?? UUID().uuidString
    }
    
    // MARK: Coding Keys
    enum CodingKeys: String, CodingKey {
        case jsonId = "$id"
        case type = "Type"
        case details = "Details"
    }
    
    // Custom initializer to provide default values
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        jsonId = try container.decodeIfPresent(String.self, forKey: .jsonId)
        type = try container.decodeIfPresent(String.self, forKey: .type)
        details = try container.decodeIfPresent(String.self, forKey: .details)
    }
}

// MARK: - Decodable Extensions

extension KeyedDecodingContainer {
    func decodeIfPresentArray<T: Decodable>(_ type: [T].Type, forKey key: K) throws -> [T] {
        return (try? decode([T].self, forKey: key)) ?? []
    }
}

