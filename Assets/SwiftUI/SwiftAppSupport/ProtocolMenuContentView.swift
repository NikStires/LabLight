import SwiftUI
import UnityFramework

struct ProtocolMenuContentView: View {
    @StateObject private var viewModel = ProtocolMenuViewModel()
    @Environment(\.dismiss) private var dismiss
    @State private var path = NavigationPath()
    
    var body: some View {
        NavigationStack(path: $path) {
            VStack {
                Text("Select a Protocol")
                    .font(.largeTitle)
                    .padding()
                
                if viewModel.protocols.isEmpty {
                    Text("Loading protocols...")
                } else {
                    List(viewModel.protocols) { protocolItem in
                        Button(action: {
                            viewModel.selectProtocol(protocolItem)
                        }) {
                            VStack(alignment: .leading, spacing: 4) {
                                Text(formatText(protocolItem.title))
                                    .font(.headline)
                                Text("Version: \(formatText(protocolItem.version))")
                                    .font(.subheadline)
                                    .foregroundColor(.gray)
                                Text(protocolItem.description)
                                    .font(.caption)
                                    .foregroundColor(.secondary)
                                    .lineLimit(2)
                            }
                            .padding(.vertical, 4)
                        }
                    }
                }
            }
            .onAppear {
                viewModel.requestProtocolDescriptions()
            }
            .navigationDestination(for: ProtocolDefinition.self) { protocolDef in
                ProtocolView(selectedProtocol: protocolDef)
            }
            .onReceive(viewModel.$selectedProtocol) { selected in
                if let selected = selected {
                    path.append(selected)
                }
            }
        }
    }
    
    func formatText(_ text: String) -> String {
        var formatted = text.components(separatedBy: CharacterSet.alphanumerics.inverted).joined()
        formatted = formatted.replacingOccurrences(of: "([a-z])([A-Z])", with: "$1 $2", options: .regularExpression, range: nil)
        return formatted.prefix(1).uppercased() + formatted.dropFirst()
    }
}

class ProtocolMenuViewModel: ObservableObject {
    @Published var protocols: [ProtocolDescriptor] = []
    @Published var selectedProtocol: ProtocolDefinition? = nil
    
    init() {
        NotificationCenter.default.addObserver(self, selector: #selector(handleProtocolDescriptions(_:)), name: Notification.Name("ProtocolDescriptions"), object: nil)
        NotificationCenter.default.addObserver(self, selector: #selector(handleProtocolChange(_:)), name: Notification.Name("ProtocolChange"), object: nil)
    }
    
    @objc func handleProtocolDescriptions(_ notification: Notification) {
        if let message = notification.userInfo?["message"] as? String,
           message.hasPrefix("protocolDescriptions|") {
            let protocolsJson = String(message.dropFirst("protocolDescriptions|".count))
            if let data: Data = protocolsJson.data(using: .utf8),
               let decodedProtocols = try? JSONDecoder().decode([ProtocolDescriptor].self, from: data) {
                DispatchQueue.main.async {
                    self.protocols = decodedProtocols
                }
            } else {
                print("######LABLIGHT Failed to decode protocols")
            }
        }
    }
    
    @objc func handleProtocolChange(_ notification: Notification) {
        if let message = notification.userInfo?["message"] as? String,
           message.hasPrefix("protocolChange|") {
            let protocolJson = String(message.dropFirst("protocolChange|".count))
            print("######LABLIGHT Received protocol JSON: \(protocolJson)")
            do {
                guard let data = protocolJson.data(using: .utf8) else {
                    print("######LABLIGHT Invalid JSON encoding")
                    return
                }
                let protocolDefinition = try JSONDecoder().decode(ProtocolDefinition.self, from: data)
                DispatchQueue.main.async {
                    self.selectedProtocol = protocolDefinition
                }
            } catch {
                print("Failed to decode JSON: \(error.localizedDescription)")
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
    
    func selectProtocol(_ protocolDescriptor: ProtocolDescriptor) {
        if let jsonData = try? JSONEncoder().encode(protocolDescriptor),
           let jsonString = String(data: jsonData, encoding: .utf8) {
            CallCSharpCallback("selectProtocol|" + jsonString)
        } else {
            print("######LABLIGHT Failed to encode protocol descriptor")
        }
    }
    
    func requestProtocolDescriptions() {
        CallCSharpCallback("requestProtocolDescriptions|")
    }
}

struct ProtocolDescriptor: Codable, Identifiable {
    let id: String
    let title: String
    let version: String
    let description: String
    
    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        title = try container.decode(String.self, forKey: .title)
        version = try container.decode(String.self, forKey: .version)
        description = try container.decode(String.self, forKey: .description)
        id = title
    }
}
