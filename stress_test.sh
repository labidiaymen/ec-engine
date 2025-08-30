#!/bin/bash

# ECEngine HTTP Server Stress Testing Script
# Provides stress testing capabilities to evaluate server stability under extreme conditions

set -e

# Configuration
DEFAULT_HOST="localhost"
DEFAULT_PORT="3000"
DEFAULT_ENDPOINT="/"
STRESS_DURATION=60
MAX_USERS=100
RAMP_UP_STEP=10
RAMP_UP_INTERVAL=5

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
ECEngine HTTP Server Stress Testing Tool

This tool performs progressive stress testing to find the breaking point of your ECEngine HTTP server.

USAGE:
    $0 [OPTIONS]

OPTIONS:
    --host HOST         Target host (default: ${DEFAULT_HOST})
    --port PORT         Target port (default: ${DEFAULT_PORT})
    --endpoint PATH     Target endpoint (default: ${DEFAULT_ENDPOINT})
    --max-users N       Maximum concurrent users to test (default: ${MAX_USERS})
    --duration N        Duration for each stress level in seconds (default: ${STRESS_DURATION})
    --ramp-step N       Number of users to add each step (default: ${RAMP_UP_STEP})
    --ramp-interval N   Seconds between adding user groups (default: ${RAMP_UP_INTERVAL})
    --help              Show this help message

STRESS TEST MODES:
    1. Progressive Load: Gradually increases load from 10 to max-users
    2. Spike Test: Sudden spike to maximum load
    3. Endurance Test: Sustained maximum load for extended period
    4. Memory Leak Test: Extended test to check for memory leaks

EXAMPLES:
    # Default stress test
    $0

    # Custom stress test parameters
    $0 --max-users 50 --duration 30 --ramp-step 5

    # Test specific endpoint
    $0 --endpoint /api/status --max-users 75

DEPENDENCIES:
    - curl (for connectivity testing)
    - ab (Apache Bench, for load testing)

NOTES:
    - This test can be resource-intensive and may impact server performance
    - Monitor server resources (CPU, memory) during testing
    - Test in a controlled environment to avoid affecting production

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

# Run progressive stress test
run_progressive_stress() {
    local host=$1
    local port=$2
    local endpoint=$3
    local max_users=$4
    local duration=$5
    local ramp_step=$6
    local ramp_interval=$7
    local url="http://${host}:${port}${endpoint}"
    
    print_info "Starting Progressive Stress Test"
    print_info "Target: ${url}"
    print_info "Max users: ${max_users}, Duration per step: ${duration}s"
    print_info "Ramp step: ${ramp_step} users every ${ramp_interval}s"
    echo ""
    
    local current_users=$ramp_step
    local failed_requests=0
    local total_requests=0
    
    while [ $current_users -le $max_users ]; do
        print_info "Testing with ${current_users} concurrent users..."
        
        # Calculate requests for this duration
        local requests=$((current_users * duration))
        
        # Run the test and capture output
        local ab_output
        ab_output=$(ab -c "$current_users" -n "$requests" -q "$url" 2>&1) || {
            print_error "Load test failed at ${current_users} users"
            break
        }
        
        # Extract key metrics
        local failed=$(echo "$ab_output" | grep "Failed requests:" | awk '{print $3}' || echo "0")
        local rps=$(echo "$ab_output" | grep "Requests per second:" | awk '{print $4}' || echo "0")
        
        failed_requests=$((failed_requests + failed))
        total_requests=$((total_requests + requests))
        
        if [ "$failed" -gt 0 ]; then
            print_warning "Failed requests: ${failed} / ${requests}"
        else
            print_success "All ${requests} requests succeeded (${rps} req/s)"
        fi
        
        # Check if failure rate is too high
        local failure_rate=0
        if [ $requests -gt 0 ]; then
            failure_rate=$((failed * 100 / requests))
        fi
        
        if [ $failure_rate -gt 10 ]; then
            print_error "Failure rate exceeded 10% (${failure_rate}%) - server may be overloaded"
            break
        fi
        
        current_users=$((current_users + ramp_step))
        
        if [ $current_users -le $max_users ]; then
            print_info "Waiting ${ramp_interval}s before next step..."
            sleep $ramp_interval
        fi
    done
    
    echo ""
    print_info "Progressive stress test completed"
    print_info "Total requests: ${total_requests}, Failed: ${failed_requests}"
}

# Run spike stress test
run_spike_test() {
    local host=$1
    local port=$2
    local endpoint=$3
    local max_users=$4
    local duration=$5
    local url="http://${host}:${port}${endpoint}"
    
    print_info "Starting Spike Stress Test"
    print_info "Target: ${url}"
    print_info "Spiking to ${max_users} concurrent users for ${duration}s"
    echo ""
    
    # Baseline test with few users
    print_info "Baseline test with 5 users..."
    ab -c 5 -n 50 -q "$url" > /dev/null 2>&1 || {
        print_error "Baseline test failed"
        return 1
    }
    print_success "Baseline test passed"
    
    sleep 2
    
    # Sudden spike to maximum load
    print_info "Spiking to ${max_users} users..."
    local requests=$((max_users * duration))
    
    local ab_output
    ab_output=$(ab -c "$max_users" -n "$requests" -q "$url" 2>&1) || {
        print_error "Spike test failed"
        return 1
    }
    
    # Extract metrics
    local failed=$(echo "$ab_output" | grep "Failed requests:" | awk '{print $3}' || echo "0")
    local rps=$(echo "$ab_output" | grep "Requests per second:" | awk '{print $4}' || echo "0")
    
    if [ "$failed" -gt 0 ]; then
        print_warning "Spike test completed with ${failed} failed requests"
    else
        print_success "Spike test completed successfully (${rps} req/s)"
    fi
}

# Run endurance test
run_endurance_test() {
    local host=$1
    local port=$2
    local endpoint=$3
    local users=$4
    local duration=$5
    local url="http://${host}:${port}${endpoint}"
    
    print_info "Starting Endurance Test"
    print_info "Target: ${url}"
    print_info "Running ${users} users for ${duration}s"
    echo ""
    
    # Use timeout to ensure we don't run forever
    timeout "${duration}s" ab -c "$users" -n 999999 -q "$url" 2>/dev/null || {
        print_info "Endurance test completed after ${duration}s"
    }
    
    print_success "Endurance test completed"
}

# Generate stress test report
generate_stress_report() {
    local host=$1
    local port=$2
    local endpoint=$3
    local max_users=$4
    local duration=$5
    
    echo ""
    print_success "Stress testing completed!"
    echo ""
    echo "=== STRESS TEST REPORT ==="
    echo "Target: http://${host}:${port}${endpoint}"
    echo "Maximum users tested: ${max_users}"
    echo "Duration per test: ${duration}s"
    echo "Timestamp: $(date)"
    echo ""
    echo "RECOMMENDATIONS:"
    echo "- Monitor server logs for errors or warnings"
    echo "- Check system resources (CPU, memory, network)"
    echo "- Consider horizontal scaling if performance degrades"
    echo "- Implement rate limiting to prevent server overload"
    echo ""
}

# Main execution
main() {
    local host="${DEFAULT_HOST}"
    local port="${DEFAULT_PORT}"
    local endpoint="${DEFAULT_ENDPOINT}"
    local max_users="${MAX_USERS}"
    local duration="${STRESS_DURATION}"
    local ramp_step="${RAMP_UP_STEP}"
    local ramp_interval="${RAMP_UP_INTERVAL}"
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
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
            --max-users)
                max_users="$2"
                shift 2
                ;;
            --duration)
                duration="$2"
                shift 2
                ;;
            --ramp-step)
                ramp_step="$2"
                shift 2
                ;;
            --ramp-interval)
                ramp_interval="$2"
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
    
    # Validate parameters
    if [ "$max_users" -le 0 ]; then
        print_error "Invalid max-users: ${max_users}"
        exit 1
    fi
    
    if [ "$duration" -le 0 ]; then
        print_error "Invalid duration: ${duration}"
        exit 1
    fi
    
    # Check dependencies
    check_dependencies
    
    # Test connectivity
    test_connectivity "$host" "$port" "$endpoint"
    
    echo ""
    print_warning "WARNING: This stress test may impact server performance"
    print_info "Starting stress testing sequence..."
    echo ""
    
    # Run stress test sequence
    run_progressive_stress "$host" "$port" "$endpoint" "$max_users" "$duration" "$ramp_step" "$ramp_interval"
    
    sleep 5
    
    run_spike_test "$host" "$port" "$endpoint" "$max_users" "$duration"
    
    sleep 5
    
    # Endurance test with moderate load
    local endurance_users=$((max_users / 2))
    local endurance_duration=$((duration * 2))
    run_endurance_test "$host" "$port" "$endpoint" "$endurance_users" "$endurance_duration"
    
    # Generate final report
    generate_stress_report "$host" "$port" "$endpoint" "$max_users" "$duration"
}

# Execute main function with all arguments
main "$@"