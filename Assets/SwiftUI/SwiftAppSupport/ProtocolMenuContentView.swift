import SwiftUI
import UnityFramework

struct ProtocolMenuContentView: View {
    @StateObject private var viewModel = ProtocolMenuViewModel()
    @Environment(\.dismiss) private var dismiss
    
    var body: some View {
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
                        dismiss()
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
    
    init() {
        print("######LABLIGHT ProtocolMenuViewModel initialized")
        NotificationCenter.default.addObserver(self, selector: #selector(handleMessage(_:)), name: Notification.Name("ProtocolMenuMessage"), object: nil)
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