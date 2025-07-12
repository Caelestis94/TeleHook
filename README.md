<div align="center" width="100%">
    <img width="100" height="100" alt="Image" src="https://github.com/user-attachments/assets/03879c91-8f7f-49b7-b23b-f2c8b8f27d75" />
</div>


# TeleHook
<img width="3838" height="2028" alt="Image" src="https://github.com/user-attachments/assets/33fa280c-4247-471b-9283-3d3a16f096e5" />


I made TeleHook to bridge the gap some of my services have when it came to Telegram notifications, some don't support topic/message thread IDs, some don't support Telegram at all. With this solution I can get the notifications I want, how I want them and where I want them with almost all services that support webhooks (which is a lot more than those who support Telegram).

## 🎯 Why TeleHook?

**Useful if you need:**
- 📡 Telegram notifications from services that don't support it natively
- 🎨 Flexible message formatting with powerful templating
- 🔐 Self-hosted solution with complete data control
- 🌐 Modern web UI for non-technical users
- 🎯 Telegram Topics support for organized notifications

**Common Use Cases:**
- **Proxmox VE** backup notifications and system alerts
- **Home Assistant** automation notifications
- **Uptime monitoring** (Uptime Kuma, Prometheus, etc.)
- **CI/CD pipelines** (GitLab, GitHub Actions, Jenkins)
- **NAS systems** (Synology, TrueNAS) event notifications

## ✨ Key Features

- **🔧 Powerful Templating**: Scriban templating with regex, string manipulation, and conditional logic
- **🎯 Multi-Bot Support**: Manage multiple Telegram bots from a single instance
- **🔍 Payload Capture**: Capture a payload from your service to setup the endpoint and the template
- **🔤 Template Editor**: Create your templates using a rich editor that includes variable auto-completion from the payload's properties/keys.
- **📊 Comprehensive Logging**: Request tracking, performance metrics, and failure notifications
- **🛡️ Flexible Security**: Optional webhook authentication with secret keys
- **⚡ Real-time Preview**: Test templates with sample data before deployment
- **🌐 Web UI**: Dashboard for configuration and monitoring
- **🔐 Auth**: OIDC/SSO integration

## 🚀 Quick Start

### Prerequisites
- Docker & Docker Compose
- Telegram Bot Token ([create one with @BotFather](https://t.me/BotFather))
- Telegram Chat ID
   - For private chats with the bot, the chat ID is your own ID
   - For groups you can get the chat ID by right clicking a message -> copy message link -> `https://t.me/c/CHAT_ID_NUMBER/MESSAGE_ID ` the first series of numbers is the chat ID, you prefix that with -100 and you get your chat ID

### 1. Download Configuration Files
```bash
# Download docker-compose.yml and example.env
curl -O https://raw.githubusercontent.com/Caelestis94/TeleHook/main/docker-compose.yml
curl -O https://raw.githubusercontent.com/Caelestis94/TeleHook/main/example.env
```

### 2. Configure Environment
```bash
# Copy and edit the environment file
cp example.env .env
nano .env
```

**Required Configuration:**
```env
# Generate a secure API key for authentication
TELEHOOK_API_KEY="your-secure-api-key-here"

# Set your desired ports
FRONTEND_PORT=3000
BACKEND_PORT=5001

# NextAuth configuration
NEXTAUTH_URL="http://localhost:3000"
NEXTAUTH_SECRET="your-nextauth-secret-here"
```

### 3. Start TeleHook
```bash
docker-compose up -d
```

### 4. Access the Dashboard
- **Web Interface**: http://localhost:3000
- **API Endpoint**: http://localhost:5001

### 5. Initial Setup
1. Navigate to the web interface
2. Complete the initial user setup
3. Add your first Telegram bot (token + chat ID)
4. Create your first webhook with a custom template

## 📝 Template Examples

Templates can be as simple or complex as you want, the better the payload sample you can give during the webhook setup, the better you can craft a template that fits your needs.

### Basic Notification
```scriban
🔔 **{{ title }}** from {{ source }}
📝 {{ message }}
```

### Advanced Proxmox Backup Report
```scriban
{{- # Extract summary info using regex -}}
{{- total_time_start = message | string.index_of "Total running time:" -}}
{{- total_size_start = message | string.index_of "Total size:" -}}

{{- if total_time_start >= 0 && total_size_start >= 0 -}}
🖥️ **{{ hostname | string.upcase }} Backup Complete**

{{- # Extract total time using regex -}}
{{- time_match = message | regex.match `Total running time:\s*([^\n\r]+)` -}}
{{- total_time = time_match.size > 1 ? time_match[1] | string.strip : "Unknown" -}}

{{- # Extract total size using regex -}}
{{- size_match = message | regex.match `Total size:\s*([^\n\r]+)` -}}
{{- total_size = size_match.size > 1 ? size_match[1] | string.strip : "Unknown" -}}

⏱️ **{{ total_time }}** | 💾 **{{ total_size }}**

{{- # Count VMs/containers using regex -}}
{{- vm_matches = message | regex.matches `^\s*(\d+)\s+[\w\-]+\s+(ok|FAILED|ERROR)` "m" -}}
{{- success_count = 0 -}}
{{- failed_count = 0 -}}

{{- for match in vm_matches -}}
{{- if match.size > 2 -}}
{{- if match[2] == "ok" -}}
{{- success_count = success_count + 1 -}}
{{- else -}}
{{- failed_count = failed_count + 1 -}}
{{- end -}}
{{- end -}}
{{- end -}}

**Status:** {{ success_count }}✅{{ if failed_count > 0 }} {{ failed_count }}❌{{ end }}
{{- end -}}
```

**Output:**
```
🖥️ PVE-N1 Backup Complete

⏱️ 5m 45s | 💾 136.355 GiB

Status: 22✅
```

## 🔧 Configuration

### Telegram Message Options
- **Parse Mode**: HTML, Markdown, or MarkdownV2
- **Silent Notifications**: Disable notification sounds
- **Web Preview**: Control link previews
- **Topics Support**: Send to specific Telegram topics

### Security Features
- **Endpoint Protection**: Secure webhooks with secret keys
- **Header/Query Authentication**: `Authorization: Bearer <key>` or `?secret_key=<key>`
- **OIDC/SSO Integration**: Enterprise authentication support

### Advanced Settings
- **Failure Notifications**: Get notified when webhooks fail
- **Request Logging**: Comprehensive audit trail
- **Background Cleanup**: Automatic log rotation

## 🔌 Integration Examples

### Proxmox VE
```bash
# Datacenter > Notifications > Add Webhook
URL: http://your-telehook:5001/api/trigger/your-webhook-uuid

# Custom Headers
Authorization: Bearer your-secret-key

Method : POST
```
Body :
```json
{
  "severity": "{{ severity }}",
  "title": "{{ escape title }}",
  "message": "{{ escape message }}",
  "hostname": "{{ fields.hostname }}",
  "metadata": {
    "job_id": "{{ fields.job-id }}",
    "event_type": "{{ fields.type }}",
    "node": "{{ fields.hostname }}",
    "severity_level": "{{ severity }}",
    "formatted_time": "{{ timestamp }}"
  }
}
```

### Uptime Kuma
```bash
# Webhook URL
http://your-telehook:5001/api/trigger/your-webhook-uuid?secret_key=your-secret

# Custom Headers
Authorization: Bearer your-secret-key

# Request Body
Custom or Prest - application/json
```

### Home Assistant
```yaml
# configuration.yaml
notify:
  - name: telehook
    platform: rest
    resource: http://your-telehook:5001/api/trigger/your-webhook-uuid
    method: POST
    headers:
      Content-Type: application/json
      Authorization: Bearer your-secret-key
```
Action
```yaml
# In your automation's action section
action:
  - service: notify.telehook
    data:
      value1: "The garage door has been open for 10 minutes."
      source: "Home Assistant Automation"
      priority: "high"
```

## 🛠️ Deployment

### Environment Variables
| Variable | Description | Default |
|----------|-------------|---------|
| `TELEHOOK_API_KEY` | API authentication key | *Required* |
| `FRONTEND_PORT` | Web interface port | `3000` |
| `BACKEND_PORT` | API port | `5001` |
| `DATABASE_PATH` | SQLite database location | `/data/telehook.db` |
| `LOG_LEVEL` | Logging verbosity | `Information` |
| `NEXTAUTH_SECRET` | NextAuth encryption key | *Required* |

## 📊 Monitoring

- **Request Logging**: Track webhook requests and performance
- **Structured Logging**: JSON logs with request correlation
- **Health Checks**: Basic Docker health monitoring
- **Future**: Prometheus metrics endpoint (planned)

## 🗺️ Roadmap

**Current Status**: Beta - works for what I need, but could use more testing

**Ideas for the future**:
- 📋 **Payload Validation**: Schema-based webhook validation
- 📈 **Prometheus Metrics**: Metrics endpoint for monitoring

## 🤝 Contributing

Feel free to contribute if you find this useful!

- **Issues**: Bug reports or feature requests
- **Pull Requests**: Code improvements welcome
- **Documentation**: Help improve the docs
- **Templates**: Share your webhook templates with others

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.
