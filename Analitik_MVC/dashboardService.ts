useEffect(() => {
    const fetchDashboard = async () => {
        try {
            setIsLoading(true);
            setError(null);

            const response = await getDashboardSummary(timeFilter);
            setData(response);

        } catch (err) {
            setError(handleDashboardError(err));
        } finally {
            setIsLoading(false);
        }
    };

    fetchDashboard();
}, [timeFilter]);