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
                    List {
                        ForEach(viewModel.protocols) { protocolDef in
                            Button(action: {
                                if let jsonData = try? JSONEncoder().encode(protocolDef),
                                   let jsonString = String(data: jsonData, encoding: .utf8) {
                                    print("######LABLIGHT Selecting protocol -> Raw protocol JSON: \(jsonString)")
                                    CallCSharpCallback("selectProtocol|" + jsonString)
                                    path.append(protocolDef)
                                }
                            }) {
                                VStack(alignment: .leading, spacing: 4) {
                                    Text(protocolDef.title)
                                        .font(.headline)
                                    Text("Version: \(protocolDef.version)")
                                        .font(.subheadline)
                                        .foregroundColor(.gray)
                                    Text(protocolDef.description)
                                        .font(.caption)
                                        .foregroundColor(.secondary)
                                        .lineLimit(2)
                                }
                                .padding(.vertical, 4)
                            }
                        }
                    }
                }
            }
            .onAppear {
                viewModel.requestProtocolDefinitions()
            }
            .navigationDestination(for: ProtocolDefinition.self) { protocolDef in
                ProtocolView(selectedProtocol: protocolDef)
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
    @Published var protocols: [ProtocolDefinition] = []
    
    init() {
        NotificationCenter.default.addObserver(self, selector: #selector(handleProtocolDefinitions(_:)), name: Notification.Name("ProtocolDefinitions"), object: nil)
    }
    
    @objc func handleProtocolDefinitions(_ notification: Notification) {
        if let message = notification.userInfo?["message"] as? String,
           message.hasPrefix("protocolDefinitions|") {
            let protocolsJson = String(message.dropFirst("protocolDefinitions|".count))
            if let data: Data = protocolsJson.data(using: .utf8) {
                do {
                    let decoder = JSONDecoder()
                    // Try to decode array elements individually
                    let jsonArray = try JSONSerialization.jsonObject(with: data) as? [[String: Any]] ?? []
                    let decodedProtocols = jsonArray.compactMap { protocolDict -> ProtocolDefinition? in
                        guard let protocolData = try? JSONSerialization.data(withJSONObject: protocolDict) else { return nil }
                        print("######LABLIGHT Raw protocol JSON: \(String(data: protocolData, encoding: .utf8) ?? "")")
                        return try? decoder.decode(ProtocolDefinition.self, from: protocolData)
                    }
                    
                    DispatchQueue.main.async {
                        self.protocols = decodedProtocols
                    }
                } catch {
                    print("######LABLIGHT Error decoding protocols: \(error)")
                }
            } else {
                print("######LABLIGHT Failed to create data from protocols JSON string")
            }
        }
    }
    
    func requestProtocolDefinitions() {
        CallCSharpCallback("requestProtocolDefinitions|")
    }
}
