function [] = PLOT_UAV_STATE(state, num)

for m = 1:num
    %altitude
    figure('Name',['Asset ',num2str(m)])
    subplot(3,1,1)
    plot(state.t{m}, state.vz{m}); hold on; grid on
    xlabel('Time (s)')
    ylabel('z-vel (m/s)')
    subplot(3,1,2)
    plot(state.t{m}, state.alt{m}); hold on; grid on
    xlabel('Time (s)')
    ylabel('Altitude (m)')

    %velocity
    figure('Name',['Asset ',num2str(m)])
    subplot(3,1,1)
    plot(state.t{m}, state.vx{m}); hold on; grid on
    xlabel('Time (s)')
    ylabel('x-vel (m/s)')
    subplot(3,1,2)
    plot(state.t{m}, state.vy{m}); hold on; grid on
    xlabel('Time (s)')
    ylabel('y-vel (m/s)')

    %angular rate
    figure('Name',['Asset ',num2str(m)])
    subplot(3,1,1)
    plot(state.t{m}, -state.p{m}*(180/pi)); hold on; grid on
    xlabel('Time (s)')
    ylabel('roll-rate (deg/s)')
    subplot(3,1,2)
    plot(state.t{m}, state.q{m}*(180/pi)); hold on; grid on
    xlabel('Time (s)')
    ylabel('pitch-rate (deg/s)')
    subplot(3,1,3)
    plot(state.t{m}, state.r{m}*(180/pi)); hold on; grid on
    xlabel('Time (s)')
    ylabel('yaw-rate (deg/s)')

    %Eueler angles
    figure('Name',['Asset ',num2str(m)])
    subplot(3,1,1)
    plot(state.t{m}, -state.phi{m}*(180/pi)); hold on; grid on
    xlabel('Time (s)')
    ylabel('roll angle (deg)')
    subplot(3,1,2)
    plot(state.t{m}, state.theta{m}*(180/pi)); hold on; grid on
    xlabel('Time (s)')
    ylabel('pitch angle (deg)')
    subplot(3,1,3)
    plot(state.t{m}, state.psi{m}*(180/pi)); hold on; grid on
    xlabel('Time (s)')
    ylabel('yaw angle (deg)')
end