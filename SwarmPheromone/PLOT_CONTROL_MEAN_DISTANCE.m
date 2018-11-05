function [] = PLOT_CONTROL_MEAN_DISTANCE(x, y)

idx = find(x >= 260);
mean_y = mean(y(idx));
max_idx = find( y == max(y(idx)) );

disp(['Average Distance: ',num2str(mean_y),' meters'])
disp(['Maximum Distance: ',num2str(y(max_idx)),' meters'])

plot( x(idx), y(idx) ); hold on; grid on
plot( [260 800],[mean_y mean_y],'--','linewidth',1)
plot(x(max_idx), y(max_idx), 'ro','linewidth',1)
xlabel('Time (s)')
ylabel('Distance (m)')
%axis([260 800 100 1000])
xlim([260 800])

figure()
plot( x, y ); grid on
xlabel('Time (s)')
ylabel('Distance (m)')