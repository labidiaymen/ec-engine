#!/bin/bash

# ECEngine HTTP Server Load Testing Script
# Provides comprehensive load testing capabilities for ECEngine HTTP server

set -e

# Configuration
DEFAULT_HOST="localhost"
DEFAULT_PORT="3000"
DEFAULT_ENDPOINT="/"
QUICK_DURATION=5
SUSTAINED_DURATION=30
DEFAULT_USERS=10
DEFAULT_REQUESTS=100

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Display usage information
show_usage() {
    cat << EOF
ECEngine HTTP Server Load Testing Tool

USAGE:
    $0 [MODE] [OPTIONS]

MODES:
    quick               Quick benchmark (${QUICK_DURATION}s test with 10 concurrent users)
    sustained           Sustained load test (${SUSTAINED_DURATION}s test with 20 concurrent users)
    custom              Custom load test (requires --users and --requests)

OPTIONS:
    --host HOST         Target host (default: ${DEFAULT_HOST})
    --port PORT         Target port (default: ${DEFAULT_PORT})
    --endpoint PATH     Target endpoint (default: ${DEFAULT_ENDPOINT})
    --users N           Number of concurrent users (for custom mode)
    --requests N        Total number of requests (for custom mode)
    --duration N        Test duration in seconds (for custom mode)
    --help              Show this help message

EXAMPLES:
    # Quick benchmark
    $0 quick

    # Sustained load test
    $0 sustained

    # Custom load test
    $0 custom --users 20 --requests 50

    # Custom load test with specific target
    $0 custom --host 127.0.0.1 --port 8080 --endpoint /api/status --users 15 --requests 100

    # Custom load test with duration
    $0 custom --users 25 --duration 60

DEPENDENCIES:
    - curl (for connectivity testing)
    - ab (Apache Bench, for load testing)

EOF
}

# Check if required tools are available
check_dependencies() {
    if ! command -v curl &> /dev/null; then
        print_error "curl is required but not installed"
        exit 1
    fi

    if ! command -v ab &> /dev/null; then
        print_error "Apache Bench (ab) is required but not installed"
        print_info "Install with: sudo apt-get install apache2-utils (Ubuntu/Debian)"
        exit 1
    fi
}

# Test server connectivity
test_connectivity() {
    local host=$1
    local port=$2
    local endpoint=$3
    local url="http://${host}:${port}${endpoint}"
    
    print_info "Testing connectivity to ${url}..."
    
    if ! curl -s --connect-timeout 5 "${url}" > /dev/null; then
        print_error "Cannot connect to ${url}"
        print_info "Make sure ECEngine HTTP server is running on ${host}:${port}"
        exit 1
    fi
    
    print_success "Server is responding"
}

# Run load test using Apache Bench
run_load_test() {
    local host=$1
    local port=$2
    local endpoint=$3
    local users=$4
    local requests=$5
    local duration=$6
    
    local url="http://${host}:${port}${endpoint}"
    
    print_info "Starting load test..."
    print_info "Target: ${url}"
    print_info "Concurrent users: ${users}"
    
    if [ -n "$duration" ]; then
        print_info "Duration: ${duration} seconds"
        print_info "Running timed load test..."
        
        # Use timeout to limit test duration
        timeout "${duration}s" ab -c "${users}" -n 999999 -q "${url}" 2>/dev/null || true
        print_info "Timed test completed"
    else
        print_info "Total requests: ${requests}"
        print_info "Requests per user: $((requests / users))"
        
        # Run standard load test
        ab -c "${users}" -n "${requests}" "${url}"
    fi
}

# Generate summary report
generate_summary() {
    local mode=$1
    local host=$2
    local port=$3
    local endpoint=$4
    local users=$5
    local requests=$6
    local duration=$7
    
    echo ""
    print_success "Load test completed!"
    echo ""
    echo "=== TEST SUMMARY ==="
    echo "Mode: ${mode}"
    echo "Target: http://${host}:${port}${endpoint}"
    echo "Concurrent users: ${users}"
    if [ -n "$duration" ]; then
        echo "Duration: ${duration} seconds"
    else
        echo "Total requests: ${requests}"
    fi
    echo "Timestamp: $(date)"
    echo ""
}

# Main execution
main() {
    local mode=""
    local host="${DEFAULT_HOST}"
    local port="${DEFAULT_PORT}"
    local endpoint="${DEFAULT_ENDPOINT}"
    local users=""
    local requests=""
    local duration=""
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            quick|sustained|custom)
                mode="$1"
                shift
                ;;
            --host)
                host="$2"
                shift 2
                ;;
            --port)
                port="$2"
                shift 2
                ;;
            --endpoint)
                endpoint="$2"
                shift 2
                ;;
            --users)
                users="$2"
                shift 2
                ;;
            --requests)
                requests="$2"
                shift 2
                ;;
            --duration)
                duration="$2"
                shift 2
                ;;
            --help)
                show_usage
                exit 0
                ;;
            *)
                print_error "Unknown option: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Validate mode
    if [ -z "$mode" ]; then
        print_error "Mode is required"
        show_usage
        exit 1
    fi
    
    # Set defaults based on mode
    case $mode in
        quick)
            users=10
            requests=100
            duration="${QUICK_DURATION}"
            ;;
        sustained)
            users=20
            requests=600
            duration="${SUSTAINED_DURATION}"
            ;;
        custom)
            if [ -z "$users" ]; then
                users="${DEFAULT_USERS}"
            fi
            if [ -z "$requests" ] && [ -z "$duration" ]; then
                requests="${DEFAULT_REQUESTS}"
            fi
            ;;
    esac
    
    # Validate parameters
    if [ -z "$users" ] || [ "$users" -le 0 ]; then
        print_error "Invalid number of users: ${users}"
        exit 1
    fi
    
    if [ -z "$duration" ] && ([ -z "$requests" ] || [ "$requests" -le 0 ]); then
        print_error "Either duration or valid number of requests is required"
        exit 1
    fi
    
    # Check dependencies
    check_dependencies
    
    # Test connectivity
    test_connectivity "$host" "$port" "$endpoint"
    
    # Run the load test
    run_load_test "$host" "$port" "$endpoint" "$users" "$requests" "$duration"
    
    # Generate summary
    generate_summary "$mode" "$host" "$port" "$endpoint" "$users" "$requests" "$duration"
}

# Execute main function with all arguments
main "$@"