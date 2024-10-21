import SwiftUI
import UnityFramework

struct ProtocolMenuContentView: View {
    @StateObject private var viewModel = ProtocolMenuViewModel()
    @Environment(\.dismiss) private var dismiss
    @State private var path = NavigationPath()  // Navigation Path State
    
    var body: some View {
        NavigationStack(path: $path) {  // Wrap content in NavigationStack
            VStack {
                Text("Select a Protocol")
                    .font(.largeTitle)
                    .padding()
                
                if viewModel.protocols.isEmpty {
                    Text("Loading protocols...")
                } else {
                    List(viewModel.protocols) { protocolItem in
                        Button(action: {
                            viewModel.selectProtocol(protocolItem.name)
                            // Optionally dismiss if using modal presentation
                            // dismiss()
                        }) {
                            VStack(alignment: .leading) {
                                Text(formatText(protocolItem.title))
                                    .font(.headline)
                                Text(formatText(protocolItem.description))
                                    .font(.subheadline)
                            }
                        }
                    }
                }
            }
            .onAppear {
                viewModel.requestProtocolDescriptions()
            }
            .navigationDestination(for: ProtocolDefinition.self) { protocolDef in
                ProtocolView(protocol: protocolDef)  // Navigate to ProtocolView
            }
            .onReceive(viewModel.$selectedProtocol) { selected in
                if let selected = selected {
                    path.append(selected)  // Append to navigation path to trigger navigation
                }
            }
        }
    }
    
    func formatText(_ text: String) -> String {
        // Remove special characters
        var formatted = text.components(separatedBy: CharacterSet.alphanumerics.inverted).joined()
        
        // Add spaces before capital letters (for camel case)
        formatted = formatted.replacingOccurrences(of: "([a-z])([A-Z])", with: "$1 $2", options: .regularExpression, range: nil)
        
        // Capitalize the first letter
        return formatted.prefix(1).uppercased() + formatted.dropFirst()
    }
}

class ProtocolMenuViewModel: ObservableObject {
    @Published var protocols: [ProtocolDescriptor] = []
    @Published var selectedProtocol: ProtocolDefinition? = nil  // Added Property
    
    init() {
        print("######LABLIGHT ProtocolMenuViewModel initialized")
        NotificationCenter.default.addObserver(self, selector: #selector(handleMessage(_:)), name: Notification.Name("ProtocolMenuMessage"), object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(handleProtocolChange(_:)), name: Notification.Name("ProtocolChange"), object: nil)
    }
    
    @objc func handleMessage(_ notification: Notification) {
        print("######LABLIGHT handleMessage called")
        if let message = notification.userInfo?["message"] as? String,
           message.hasPrefix("protocolDescriptions:") {
            let protocolsJson = String(message.dropFirst("protocolDescriptions:".count))
            if let data = protocolsJson.data(using: .utf8),
               let decodedProtocols = try? JSONDecoder().decode([ProtocolDescriptor].self, from: data) {
                DispatchQueue.main.async {
                    self.protocols = decodedProtocols
                    print("######LABLIGHT Protocols updated: \(self.protocols.count)")
                }
            } else {
                print("######LABLIGHT Failed to decode protocols")
            }
        }
    }
    
    @objc func handleProtocolChange(_ notification: Notification) {
        print("######LABLIGHT handleProtocolChange called")
        if let message = notification.userInfo?["message"] as? String,
           message.hasPrefix("protocolChange:") {
            let protocolJson = String(message.dropFirst("protocolChange:".count))
            print("######LABLIGHT Received protocol JSON: \(protocolJson)")
            do {
                guard let data = protocolJson.data(using: .utf8) else {
                    print("######LABLIGHT Invalid JSON encoding")
                    return
                }
                let protocolDefinition = try JSONDecoder().decode(ProtocolDefinition.self, from: data)
                // Successfully decoded, assign to selectedProtocol to trigger navigation
                DispatchQueue.main.async {
                    self.selectedProtocol = protocolDefinition
                    print("Protocol Title: \(protocolDefinition.title)")
                }
            } catch {
                print("Failed to decode JSON: \(error.localizedDescription)")
                // For more detailed error information:
                if let decodingError = error as? DecodingError {
                    switch decodingError {
                    case .typeMismatch(let type, let context):
                        print("Type '\(type)' mismatch:", context.debugDescription)
                        print("CodingPath:", context.codingPath)
                    case .valueNotFound(let type, let context):
                        print("Value '\(type)' not found:", context.debugDescription)
                        print("CodingPath:", context.codingPath)
                    case .keyNotFound(let key, let context):
                        print("Key '\(key)' not found:", context.debugDescription)
                        print("CodingPath:", context.codingPath)
                    case .dataCorrupted(let context):
                        print("Data corrupted:", context.debugDescription)
                        print("CodingPath:", context.codingPath)
                    @unknown default:
                        print("Unknown decoding error")
                    }
                }
            }
        }
    }
    
    func requestProtocolDescriptions() {
        print("######LABLIGHT requestProtocolDescriptions called")
        CallCSharpCallback("requestProtocolDescriptions:")
    }
    
    func selectProtocol(_ name: String) {
        CallCSharpCallback("selectProtocol:" + name)
    }
}

struct ProtocolDescriptor: Codable, Identifiable {
    let id: String
    let title: String
    let name: String
    let description: String
    
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        title = try container.decode(String.self, forKey: .title)
        name = try container.decode(String.self, forKey: .name)
        description = try container.decode(String.self, forKey: .description)
        id = name // Use name as the id
    }
}
