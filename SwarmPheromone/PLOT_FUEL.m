function [] = PLOT_FUEL(t, fuel, found, num)

figure()
subplot(3,1,1:2)
stairs(t,fuel); grid on
xlabel('Time (s)')
ylabel('Fuel Consumption (gal)')

figure()
subplot(3,1,1:2)
for i = 1:num
    stairs(t,found(:,2,i),'linewidth',1); hold on; grid on
end
ylim([0 1.5])
xlabel('Time (s)')
ylabel('Target Found')