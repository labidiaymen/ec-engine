# ECEngine HTTP Server Load Testing Tools

This directory contains comprehensive load testing and stress testing tools specifically designed for ECEngine HTTP servers.

## 📁 Files

- **`load_test.sh`** - Primary load testing script with multiple modes
- **`../stress_test.sh`** - Advanced stress testing for stability evaluation  
- **`README.md`** - This documentation file

## 🚀 Quick Start

### Prerequisites

Ensure you have the required tools installed:
```bash
# Check if tools are available
curl --version
ab -V

# Install Apache Bench if needed (Ubuntu/Debian)
sudo apt-get install apache2-utils
```

### Start Your ECEngine Server

First, start your ECEngine HTTP server:
```bash
# Example: Start the basic HTTP server
cd /path/to/ec-engine
./bin/Debug/net8.0/ECEngine examples/http_server_example.ec
```

### Run Load Tests

```bash
# Quick benchmark (5 seconds, 10 users)
./load_test.sh quick

# Sustained load test (30 seconds, 20 users)  
./load_test.sh sustained

# Custom load test
./load_test.sh custom --users 20 --requests 50
```

## 🔧 Load Testing Tool (`load_test.sh`)

### Modes

| Mode | Description | Duration | Users | Use Case |
|------|-------------|----------|-------|----------|
| `quick` | Fast benchmark | 5 seconds | 10 | Quick performance check |
| `sustained` | Sustained load | 30 seconds | 20 | Normal load simulation |
| `custom` | Custom parameters | Configurable | Configurable | Specific testing needs |

### Options

- `--host HOST` - Target host (default: localhost)
- `--port PORT` - Target port (default: 3000)
- `--endpoint PATH` - Target endpoint (default: /)
- `--users N` - Number of concurrent users
- `--requests N` - Total number of requests
- `--duration N` - Test duration in seconds
- `--help` - Show detailed help

### Examples

```bash
# Test different endpoints
./load_test.sh quick --endpoint /api/status

# Test with custom parameters
./load_test.sh custom --users 25 --requests 200

# Test different server
./load_test.sh sustained --host 192.168.1.100 --port 8080

# Duration-based test
./load_test.sh custom --users 15 --duration 45
```

## 💥 Stress Testing Tool (`../stress_test.sh`)

The stress testing tool performs progressive load testing to find your server's breaking point.

### Features

- **Progressive Load**: Gradually increases from 10 to max users
- **Spike Testing**: Sudden load spikes to test resilience  
- **Endurance Testing**: Sustained load over extended periods
- **Automatic Failure Detection**: Stops when error rate exceeds 10%

### Usage

```bash
# Default stress test (up to 100 users)
../stress_test.sh

# Custom stress test parameters
../stress_test.sh --max-users 50 --duration 30 --ramp-step 5

# Test specific endpoint
../stress_test.sh --endpoint /api/data --max-users 75
```

### Options

- `--max-users N` - Maximum concurrent users (default: 100)
- `--duration N` - Duration per stress level (default: 60s)
- `--ramp-step N` - Users to add each step (default: 10)
- `--ramp-interval N` - Seconds between steps (default: 5)

## 📊 Understanding Results

### Apache Bench Output

Key metrics to monitor:

- **Requests per second** - Server throughput
- **Time per request** - Response latency
- **Failed requests** - Error rate
- **Connection times** - Network performance

### Example Output

```
Concurrency Level:      10
Time taken for tests:   2.345 seconds
Complete requests:      100
Failed requests:        0
Total transferred:      23400 bytes
Requests per second:    42.64 [#/sec] (mean)
Time per request:       234.5 [ms] (mean)
```

## 🎯 Testing Strategy

### 1. Development Testing
```bash
# Quick verification during development
./load_test.sh quick
```

### 2. Performance Baseline
```bash
# Establish performance baseline
./load_test.sh sustained
```

### 3. Capacity Planning
```bash
# Find server limits
../stress_test.sh --max-users 200
```

### 4. Regression Testing
```bash
# Compare performance after changes
./load_test.sh custom --users 50 --requests 500
```

## 🔍 Troubleshooting

### Common Issues

**Connection Refused**
```bash
# Check if server is running
curl http://localhost:3000/

# Verify server status
ps aux | grep ECEngine
```

**High Error Rate**
- Reduce concurrent users
- Check server logs for errors
- Monitor system resources

**Tool Not Found**
```bash
# Install Apache Bench
sudo apt-get install apache2-utils

# Or use alternative tools
# hey: go install github.com/rakyll/hey@latest
# wrk: sudo apt-get install wrk
```

## 📈 Performance Optimization Tips

### Server-Side
- Monitor CPU and memory usage during tests
- Check for memory leaks with extended testing
- Implement connection pooling if needed
- Consider async request handling

### Test Configuration
- Start with low load and gradually increase
- Test different endpoints and request types
- Use consistent test environments
- Document baseline performance metrics

## 🧪 Integration with CI/CD

### Basic Performance Gate
```bash
#!/bin/bash
# Add to your CI pipeline

# Start server in background
./bin/Debug/net8.0/ECEngine examples/http_server_example.ec &
SERVER_PID=$!

# Wait for server to start
sleep 5

# Run performance test
./examples/performance/load_test.sh quick

# Cleanup
kill $SERVER_PID
```

### Advanced Monitoring
```bash
# Performance regression detection
./load_test.sh sustained 2>&1 | grep "Requests per second" | \
awk '{if($4 < 50) exit 1}' # Fail if RPS < 50
```

## 📚 Additional Resources

- [Apache Bench Documentation](https://httpd.apache.org/docs/2.4/programs/ab.html)
- [Load Testing Best Practices](https://www.blazemeter.com/blog/load-testing-best-practices)
- [ECEngine Documentation](../README.md)

## 🤝 Contributing

To improve these testing tools:

1. Test with different ECEngine server configurations
2. Add support for additional load testing tools
3. Enhance reporting and metrics collection
4. Add integration with monitoring systems

## 📝 License

These tools are part of the ECEngine project and follow the same license terms.