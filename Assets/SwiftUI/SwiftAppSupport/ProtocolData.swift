import Foundation
import Combine

// MARK: - Protocol Definition

struct ProtocolDefinition: Codable, Identifiable, Equatable, Hashable {
    let version: String
    let title: String
    let description: String
    let protocolPDFNames: [String]
    let globalArObjects: [ArObject]
    let steps: [StepDefinition]
    let mediaBasePath: String?
    
    // Computed property for Identifiable
    var id: String { title }
    
    // MARK: Coding Keys
    enum CodingKeys: String, CodingKey {
        case version, title, description, protocolPDFNames, globalArObjects, steps, mediaBasePath
    }
    
    // MARK: Hashable
    func hash(into hasher: inout Hasher) {
        hasher.combine(id)
        hasher.combine(version)
    }
    
    // MARK: Equatable
    static func == (lhs: ProtocolDefinition, rhs: ProtocolDefinition) -> Bool {
        return lhs.id == rhs.id &&
               lhs.version == rhs.version &&
               lhs.title == rhs.title &&
               lhs.description == rhs.description &&
               lhs.protocolPDFNames == rhs.protocolPDFNames &&
               lhs.globalArObjects == rhs.globalArObjects &&
               lhs.steps == rhs.steps &&
               lhs.mediaBasePath == rhs.mediaBasePath
    }
}

// MARK: - Step Definition

struct StepDefinition: Codable, Identifiable, Equatable, Hashable {
    let isCritical: Bool
    let estimatedDurationInSeconds: Int
    let contentItems: [ContentItem]
    let checklist: [CheckItemDefinition]
    
    // Computed property for Identifiable
    let id: UUID = UUID()
    
    private enum CodingKeys: String, CodingKey {
        case isCritical
        case estimatedDurationInSeconds
        case contentItems
        case checklist
    }
    
    // MARK: Hashable
    func hash(into hasher: inout Hasher) {
        hasher.combine(id)
    }
    
    // MARK: Equatable
    static func == (lhs: StepDefinition, rhs: StepDefinition) -> Bool {
        return lhs.id == rhs.id &&
               lhs.isCritical == rhs.isCritical &&
               lhs.estimatedDurationInSeconds == rhs.estimatedDurationInSeconds &&
               lhs.contentItems == rhs.contentItems &&
               lhs.checklist == rhs.checklist
    }
}

// MARK: - Check Item Definition

struct CheckItemDefinition: Codable, Identifiable, Equatable, Hashable {
    let text: String
    let contentItems: [ContentItem]
    
    // Computed property for Identifiable
    let id: UUID = UUID()
    
    enum CodingKeys: String, CodingKey {
        case text = "Text"
        case contentItems
    }
    
    // MARK: Hashable
    func hash(into hasher: inout Hasher) {
        hasher.combine(id)
    }
    
    // MARK: Equatable
    static func == (lhs: CheckItemDefinition, rhs: CheckItemDefinition) -> Bool {
        return lhs.id == rhs.id &&
               lhs.text == rhs.text &&
               lhs.contentItems == rhs.contentItems
    }
}

// MARK: - Content Item

struct ContentItem: Codable, Identifiable, Equatable, Hashable {
    let contentType: String
    let properties: [String: String]
    let arObjectID: String
    
    let id: UUID = UUID()
    
    private enum CodingKeys: String, CodingKey {
        case contentType
        case properties
        case arObjectID
    }
    
    // Add init to handle missing arObjectID
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        contentType = try container.decode(String.self, forKey: .contentType)
        properties = try container.decode([String: String].self, forKey: .properties)
        arObjectID = try container.decodeIfPresent(String.self, forKey: .arObjectID) ?? ""
    }
    
    // MARK: Hashable
    func hash(into hasher: inout Hasher) {
        hasher.combine(id)
    }
    
    // MARK: Equatable
    static func == (lhs: ContentItem, rhs: ContentItem) -> Bool {
        return lhs.id == rhs.id &&
               lhs.contentType == rhs.contentType &&
               lhs.properties == rhs.properties &&
               lhs.arObjectID == rhs.arObjectID
    }
}

// MARK: - AR Object

struct ArObject: Codable, Identifiable, Equatable, Hashable {
    let specificObjectName: String
    let arObjectID: String
    let rootPrefabName: String
    
    var id: String { arObjectID }
    
    private enum CodingKeys: String, CodingKey {
        case specificObjectName
        case arObjectID
        case rootPrefabName
    }
}
